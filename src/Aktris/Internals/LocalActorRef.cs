using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading;
using Aktris.Dispatching;
using Aktris.Exceptions;
using Aktris.Internals.Children;
using Aktris.Internals.Helpers;
using Aktris.Internals.Path;
using Aktris.Internals.SystemMessages;
using Aktris.JetBrainsAnnotations;

namespace Aktris.Internals
{
	public class LocalActorRef : InternalActorRef
	{
		internal const uint UndefinedInstanceId = 0;

		private readonly ActorSystem _system;
		private readonly ActorInstantiator _actorInstantiator;
		private readonly ActorPath _path;
		private readonly Mailbox _mailbox;
		private readonly InternalActorRef _supervisor;
		private Actor _actor;
		private readonly SenderActorRef _deadLetterSender;
		private Envelope _currentMessage;
		private volatile ChildrenCollection _childrenDoNotCallMeDirectly = EmptyChildrenCollection.Instance;


		public LocalActorRef([NotNull] ActorSystem system, [NotNull] ActorInstantiator actorInstantiator, [NotNull] ActorPath path, [NotNull] Mailbox mailbox, [NotNull] InternalActorRef supervisor)
		{
			if(system == null) throw new ArgumentNullException("system");
			if(actorInstantiator == null) throw new ArgumentNullException("actorInstantiator");
			if(path == null) throw new ArgumentNullException("path");
			if(mailbox == null) throw new ArgumentNullException("mailbox");
			if(supervisor == null) throw new ArgumentNullException("supervisor");
			_system = system;
			_actorInstantiator = actorInstantiator;
			_path = path;
			_mailbox = mailbox;
			_supervisor = supervisor;
			SendSystemMessage(new CreateActor(), this);
			supervisor.SendSystemMessage(new SuperviseActor(this),this);
			_deadLetterSender = new SenderActorRef(system.DeadLetters, this);
		}

		public string Name { get { return _path.Name; } }
		public ActorPath Path { get { return _path; } }
		public uint InstanceId { get { return _path.InstanceId; } }

		public ActorSystem System { get { return _system; } }

		public Mailbox Mailbox { get { return _mailbox; } }
		protected Envelope CurrentMessage { get { return _currentMessage; } }

		public void Start()
		{
			_mailbox.SetActor(this);
		}

		public void Stop()
		{			
		}

		public void Restart(Exception causedByFailure)
		{			
		}

		public void Send(object message, ActorRef sender)
		{
			sender = UnwrapSenderActorRef(sender);
			var envelope = new Envelope(this, message, sender ?? _system.DeadLetters);
			_mailbox.Enqueue(envelope);
		}

		public void SendSystemMessage(SystemMessage message, ActorRef sender)
		{
			sender = UnwrapSenderActorRef(sender);
			var envelope = new SystemMessageEnvelope(this, message, sender ?? _system.DeadLetters);
			_mailbox.EnqueueSystemMessage(envelope);
		}

		private ActorRef UnwrapSenderActorRef(ActorRef sender)
		{
			var senderActorRef = sender as SenderActorRef;
			return senderActorRef != null ? senderActorRef.Unwrap() : sender;
		}


		public void HandleMessage(Envelope envelope)
		{
			try
			{
				_currentMessage = envelope;
				_actor.Sender = new SenderActorRef(envelope.Sender, this);
				_actor.HandleMessage(envelope.Message);
			}
			catch(Exception ex)
			{
				if(!HandleInvokeFailure(ex)) throw;
			}
			finally
			{
				_currentMessage = null;
				_actor.Sender = _deadLetterSender;	//TODO: change to use one that directs to deadletter
			}

		}

		public void HandleSystemMessage(SystemMessageEnvelope envelope)
		{
			var message = envelope.Message;
			var wasMatched =
				IfMatchSys<CreateActor>(message, CreateActorInstance)
				|| IfMatchSys<SuspendActor>(message, _ =>{SuspendThisOnly();SuspendChildren();})
				|| IfMatchSys<ActorFailed>(message, HandleActorFailure)
				|| IfMatchSys<SuperviseActor>(message, superviseMessage => Supervise(superviseMessage.ActorToSupervise));
			if(!wasMatched)
			{
				//This should never happen. If it does a new SystemMessage type has been added
				throw new InvalidOperationException(string.Format("Unexpected system message type: {0}. Message was: {1}", message.GetType(), message));
			}
		}


		private void CreateActorInstance(CreateActor obj)
		{
			try
			{
				LocalActorRefStack.PushActorRefToStack(this);
				var actor = NewActorInstance();
				actor.Init();
				actor.PreStart();
				_actor = actor;
			}
			catch(Exception ex)
			{
				throw new CreateActorFailedException(this, "An error occured while creating the actor. See inner exception", ex);
			}
			finally
			{
				LocalActorRefStack.PopActorAndMarkerFromStack();
			}
		}

		private Actor NewActorInstance()
		{
			var actor = _actorInstantiator.CreateNewActor();
			if(actor == null) throw new CreateActorFailedException(this, "CreateNewActor returned null");
			return actor;
		}

		public virtual ActorRef CreateActor(ActorCreationProperties actorCreationProperties, string name = null)
		{
			if(name != null)
			{
				ActorNameValidator.EnsureNameIsValid(name);
			}
			else name = _system.UniqueNameCreator.GetNextRandomName();
			return CreateLocalActorReference(actorCreationProperties, name);
		}

		protected InternalActorRef CreateLocalActorReference(ActorCreationProperties actorCreationProperties, string name)
		{
			return CreateLocalActorReference(actorCreationProperties, name, this);
		}

		private InternalActorRef CreateLocalActorReference(ActorCreationProperties actorCreationProperties, string name, InternalActorRef supervisor)
		{
			InternalActorRef actorRef;
			try
			{
				ReserveChild(name);
				var instanceId = CreateInstanceId();
				var path = new ChildActorPath(_path, name, instanceId);
				actorRef = _system.LocalActorRefFactory.CreateActor(_system, actorCreationProperties, supervisor, path);
			}
			catch
			{
				ReleaseChild(name);
				throw;
			}
			actorRef.Start();
			return actorRef;
		}

		private static uint CreateInstanceId()
		{
			uint nextInstanceId;
			do
			{
				nextInstanceId = RandomProvider.GetNextUInt();
			} while(nextInstanceId == UndefinedInstanceId);
			return nextInstanceId;
		}



		// Supervision -------------------------------------------------------------------------------------

		private void Supervise(InternalActorRef actor)
		{
			ChildInfo childInfo;
			var actorName = actor.Name;

			if(Children.TryGetByName(actorName, out childInfo))
			{
				var childRestartInfo = childInfo as ChildRestartInfo;
				if(childRestartInfo == null)
				{
					childRestartInfo=new ChildRestartInfo(actor);
					UpdateChildrenCollection(c => c.AddOrUpdate(actorName, childRestartInfo));
				}
			}
			else
			{
				//TODO: publish(Error(self.path.toString, clazz(actor), "received Supervise from unregistered child " + child + ", this will not end well"))
			}
			// if (!isTerminating) {
			//	// Supervise is the first thing we get from a new child, so store away the UID for later use in handleFailure()
			//	initChild(child) match {
			//		case Some(crs) ⇒
			//			handleSupervise(child, async)
			//			if (system.settings.DebugLifecycle) publish(Debug(self.path.toString, clazz(actor), "now supervising " + child))
			//		case None ⇒ publish(Error(self.path.toString, clazz(actor), "received Supervise from unregistered child " + child + ", this will not end well"))
			//	}
			//}
		}


		// Failure -------------------------------------------------------------------------------------
		private ActorRef _failPerpatrator = null;
		private bool HandleInvokeFailure(Exception exception, IImmutableEnumerable<ActorRef> childrenNotToSuspend=null)
		{
			if(!IsFailed)
			{
				SuspendThisOnly();
				var failedMessage = new SystemMessageEnvelope(_supervisor, new ActorFailed(this, exception), this);
				_supervisor.HandleSystemMessage(failedMessage);
				var childrenToSkip = childrenNotToSuspend == null ? new HashSet<ActorRef>() : new HashSet<ActorRef>(childrenNotToSuspend);
				var ignored=
					PatternMatcher.Match<ActorFailed>(CurrentMessage.Message,m=> { SetFailedPerpatrator(m.Child);childrenToSkip.Add(m.Child);})
					|| PatternMatcher.MatchAll(CurrentMessage.Message,m=> SetFailedPerpatrator(this));
				SuspendChildren(childrenToSkip);
			}
			return true;
		}

		private void SuspendChildren(ISet<ActorRef> childrenToSkip=null)
		{
			var childrenToSuspend = childrenToSkip.IsNullOrEmpty() ? Children : Children.ExceptThoseInSet(childrenToSkip);
			childrenToSuspend.ForEach(a => a.Suspend());
		}

		void InternalActorRef.Suspend()
		{
			try
			{
				_mailbox.EnqueueSystemMessage(new SystemMessageEnvelope(this,SuspendActor.Instance,this));
			}
			catch(Exception e)
			{
				//TODO: Log
			}
		}

		void InternalActorRef.Resume(Exception causedByFailure)
		{
			try
			{
				_mailbox.EnqueueSystemMessage(new SystemMessageEnvelope(this, new ResumeActor(causedByFailure), this));
			}
			catch(Exception e)
			{
				//TODO: Log
			}
		}

		private void HandleActorFailure(ActorFailed actorFailedMessage)
		{
			//_currentMessage=new Envelope(this,actorFailedMessage, actorFailedMessage.Child);
			ChildRestartInfo childInfo;
			var failedChild =(InternalActorRef) actorFailedMessage.Child;
			if(!Children.TryGetByRef(failedChild, out childInfo))
			{
				//TODO: Log string.Format("Dropping Failed({0}) from unknown child {1}", cause, failedChild)));
			}
			else if(childInfo.Child.InstanceId != failedChild.InstanceId)
			{
				//TODO: string.Format("Dropping Failed({0}) from old child {1} (Instance id={2} != {3})", cause, failedChild, childInfo.Child.InstanceId, failedChild.InstanceId)));
			}
			else
			{
				//if(!_actor.SupervisorStrategy.HandleFailure(this, failedChild, cause, childStats, GetAllChildStats()))
				//	throw cause;
			}
		}


		private void SuspendThisOnly() { _mailbox.Suspend(this); }
		private void ResumeThisOnly() { _mailbox.Resume(this); }

		private bool IsFailed { get { return _failPerpatrator != null; } }

		protected ChildrenCollection Children { get { return _childrenDoNotCallMeDirectly; } }

		private void SetFailedPerpatrator(ActorRef perpetrator) { _failPerpatrator = perpetrator; }

		// Children -------------------------------------------------------------------------------------
		private void ReserveChild(string name)
		{
			UpdateChildrenCollection(c => c.ReserveName(name));
		}

		private void ReleaseChild(string name)
		{
			UpdateChildrenCollection(c => c.ReleaseName(name));
		}

		private ChildrenCollection UpdateChildrenCollection(Func<ChildrenCollection, ChildrenCollection> updater)
		{
#pragma warning disable 420		//Ok to disregard from CS0420 "a reference to a volatile field will not be treated as volatile" as we're using interlocked underneath, see http://msdn.microsoft.com/en-us/library/4bw5ewxy.aspx
			return InterlockedSpin.Swap(ref _childrenDoNotCallMeDirectly, updater);
#pragma warning restore 420
		}

		/// <summary>If the message is of the specified type then the handler is invoked and true is returned. </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool IfMatchSys<T>(SystemMessage message, Action<T> handler) where T : class, SystemMessage
		{
			return PatternMatcher.Match<T>(message, handler);
		}
	}

	public class GuardianActorRef : LocalActorRef
	{
		public GuardianActorRef([NotNull] ActorSystem system, [NotNull] ActorInstantiator actorInstantiator, [NotNull] ActorPath path, [NotNull] Mailbox mailbox, [NotNull] InternalActorRef supervisor)
			: base(system, actorInstantiator, path, mailbox, supervisor)
		{
		}

		public InternalActorRef CreateGuardian(Func<Actor> actorFactory, string name)
		{
			var props = new DelegateActorCreationProperties(actorFactory);
			var guardian = CreateLocalActorReference(props, name);
			return guardian;
		}
	}

	public class RootGuardianSupervisor : EmptyLocalActorRef
	{
		private const string _Name = "_Root-guardian-supervisor";
		public override string Name { get { return _Name; } }

		public RootGuardianSupervisor(RootActorPath root):base(new ChildActorPath(root,_Name,LocalActorRef.UndefinedInstanceId))
		{
		}

		public override ActorRef CreateActor(ActorCreationProperties actorCreationProperties, string name = null)
		{
			throw new InvalidOperationException(string.Format("Creating children to {0} is not allowed.", GetType()));
		}

		public override void SendSystemMessage(SystemMessage message, ActorRef sender)
		{
			var ignored = PatternMatcher.Match<SuperviseActor>(message, _ => { })
			              || PatternMatcher.MatchAll(message, _ =>
			              {
				              //TODO: Log "Recevied unexpected system message of type {0}: {1}", message.GetType(), message
			              });
		}
	}
}