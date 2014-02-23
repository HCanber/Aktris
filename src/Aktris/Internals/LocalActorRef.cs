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
using Aktris.Messages;

namespace Aktris.Internals
{

	public class LocalActorRef : InternalActorRef
	{
		internal const uint UndefinedInstanceId = 0;

		private readonly ActorSystem _system;
		private readonly ActorInstantiator _actorInstantiator;
		private readonly ActorPath _path;
		private Mailbox _mailbox;
		private readonly InternalActorRef _supervisor;
		private Actor _actor;
		private readonly SenderActorRef _deadLetterSender;
		private Envelope _currentMessage;
		private volatile ChildrenCollection _childrenDoNotCallMeDirectly = EmptyChildrenCollection.Instance;
		private ActorStatus _actorStatus = ActorStatus.Normal;
		private static readonly IImmutableSet<InternalActorRef> _EmptyActorRefSet = ImmutableHashSet<InternalActorRef>.Empty;
		private IImmutableSet<InternalActorRef> _watching = _EmptyActorRefSet;
		private IImmutableSet<InternalActorRef> _watchedBy = _EmptyActorRefSet;

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
			supervisor.SendSystemMessage(new SuperviseActor(this), this);
			_deadLetterSender = new SenderActorRef(system.DeadLetters, this);
		}

		public string Name { get { return _path.Name; } }
		public ActorPath Path { get { return _path; } }
		public uint InstanceId { get { return _path.InstanceId; } }

		public ActorSystem System { get { return _system; } }

		public Mailbox Mailbox { get { return _mailbox; } }
		protected Envelope CurrentMessage { get { return _currentMessage; } }

		public bool IsTerminated { get { return _mailbox.IsClosed; } }
		void InternalActorRef.Start()
		{
			_mailbox.SetActor(this);
		}

		public void Stop()
		{
			SafeEnqueueSystemMessage(TerminateActor.Instance);
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
				var message = envelope.Message;
				var autoHandledMessage = message as AutoHandledMessage;
				if(autoHandledMessage != null)
				{
					AutoHandleMessage(envelope);
				}
				else
				{
					_actor.Sender = new SenderActorRef(envelope.Sender, this);
					_actor.HandleMessage(message);
				}
			}
			catch(Exception ex)
			{
				if(!EscalateError(ex)) throw;
			}
			finally
			{
				_currentMessage = null;
				_actor.Sender = _deadLetterSender; //TODO: change to use one that directs to deadletter
			}

		}

		private void AutoHandleMessage(Envelope envelope)
		{
			var message = envelope.Message;
			if(message is StopActor)
			{
				Stop();
			}
			else
			{
				throw new InvalidOperationException(string.Format("Trying to AutoHandle message {0}. No handler exists for {1}", envelope, message.GetType()));
			}
		}

		public void HandleSystemMessage(SystemMessageEnvelope envelope)
		{
			try
			{
				var message = envelope.Message;

				// ReSharper disable ConvertClosureToMethodGroup   Reason: http://vibrantcode.com/2013/02/19/lambdas-vs-method-groups/
				var wasMatched =
					IfMatchSys<CreateActor>(message, _ => CreateActorInstance())
					|| IfMatchSys<SuspendActor>(message, _ =>
					{
						SuspendThisOnly();
						SuspendChildren();
					})
					|| IfMatchSys<ActorFailed>(message, m => HandleActorFailure(m))
					|| IfMatchSys<TerminateActor>(message, m => HandleTerminateActor())
					|| IfMatchSysCause<RecreateActor>(message, m => RecreateActor(m))
					|| IfMatchSys<SuperviseActor>(message, superviseMessage => Supervise(superviseMessage.ActorToSupervise))
					|| IfMatchSys<ActorTerminated>(message, m => HandleActorTerminated(m))
					|| IfMatchSys<WatchActor>(message, m => HandleWatch(m))
					|| IfMatchSys<UnwatchActor>(message, m => HandleUnwatch(m));
				// ReSharper restore ConvertClosureToMethodGroup
				if(!wasMatched)
				{
					//This should never happen. If it does a new SystemMessage type has been added without updating the code
					throw new InvalidOperationException(String.Format("Unexpected system message type: {0}. Message was: {1}", message.GetType(), message));
				}
			}
			catch(Exception e)
			{
				EscalateError(e);
			}
		}



		private Actor CreateActorInstance(Exception recreateCause = null)
		{
			try
			{
				var actor = NewActorInstance();
				actor.Init(this); //Init is idempotent so even if NewActorInstance() returns the same instance again this call is safe to make.
				_actorStatus = ActorStatus.Normal;
				actor.PreStart();
				if(recreateCause == null) actor.PreFirstStart();
				else actor.PostRestart(recreateCause);
				return actor;
			}
			catch(Exception ex)
			{
				throw new CreateActorFailedException(this, "An error occured while creating the actor. See inner exception", ex);
			}
		}

		private Actor NewActorInstance()
		{
			try
			{
				LocalActorRefStack.PushActorRefToStack(this);

				var actor = _actorInstantiator.CreateNewActor();
				_actor = actor;
				if(actor == null) throw new CreateActorFailedException(this, "CreateNewActor returned null");
				return actor;
			}
			finally
			{
				LocalActorRefStack.PopActorAndMarkerFromStack();
			}

		}

		private void RecreateActor(Exception cause)
		{
			if(_actor == null)
			{
				//The actor has not yet been created. We can use a simpler approach
				//TODO: Log 
				CreateActorInstanceDueToFailure();
			}
			else if(_actorStatus.IsNotCreatingRecreatingOrTerminating)
			{
				var failedActor = _actor;
				if(failedActor != null)
				{
					//TODO: Stash optional message
					var optionalMessage = _currentMessage != null ? _currentMessage.Message : null;
					try
					{
						failedActor.PreRestart(cause, optionalMessage); //By default this will unwatch and stop children
						failedActor.PostStop();
					}
					catch(Exception)
					{
						//TODO: Log
					}
					finally
					{
						failedActor.Clear();
						_currentMessage = null;
					}
					Debug.Assert(_mailbox.IsSuspended, "Mailbox must be suspended during restart, status=" + (_mailbox is MailboxBase ? (_mailbox as MailboxBase).GetMailboxStatusForDebug() : "unknown"));

					//If we have children we must wait for, then set status to recreating, so we know what to do when all children have terminated
					var weHaveChildrenWeMustWaitFor = InterlockedSpin.ConditionallySwap(ref _actorStatus, _ => Children.HasChildrenThatAreTerminating(), _ => ActorStatus.Recreating(cause));
					if(!weHaveChildrenWeMustWaitFor)
					{
						//No children we must wait for. Continue with recreating
						FinishRecreateActor(failedActor, cause);
					}
				}
			}
			else
			{
				ResumeActor(null);
			}
		}

		public void UnwatchAndStopChildren()
		{
			Children.GetChildrenRefs().ForEach(c =>
			{
				Unwatch(c);
				StopChild(c);
			});
		}

		private void FinishRecreateActor(Actor failedActor, Exception cause)
		{
			var survivors = Children.GetChildrenRefs().ToImmutableEnumerable();
			try
			{
				try
				{
					ResumeThisOnly();
				}
				finally
				{
					ClearFailedPerpatrator();
				}
				var freshActor = CreateActorInstance(cause);
				freshActor.PostRestart(cause);

				//Restart children
				survivors.ForEach(c => c.Restart(cause), (c, e) =>
				{
					/*TODO: Log */
				});
			}
			catch(Exception e)
			{
				_actor.Clear();
				EscalateError(e, survivors);
			}
		}

		private void CreateActorInstanceDueToFailure()
		{
			Debug.Assert(_mailbox.IsSuspended, "Mailbox must be suspended during restart, status=" + (_mailbox is MailboxBase ? (_mailbox as MailboxBase).GetMailboxStatusForDebug() : "unknown"));
			Debug.Assert(_failPerpatrator == this, "Perpetrator should be this instance");

			//Stop all children
			Children.GetChildrenRefs().ForEach(c => c.Stop());

			//If we have children we must wait for, then set status to creating, so we know what to do when all children have terminated
			var weHaveChildrenWeMustWaitFor = InterlockedSpin.ConditionallySwap(ref _actorStatus, _ => Children.HasChildrenThatAreTerminating(), ActorStatus.Creating);

			if(!weHaveChildrenWeMustWaitFor)
			{
				FinishCreateActorInstanceDueToFailure();
			}
		}

		private void FinishCreateActorInstanceDueToFailure()
		{
			try
			{
				ResumeThisOnly();
			}
			finally
			{
				ClearFailedPerpatrator();
			}
			try
			{
				CreateActorInstance();
			}
			catch(Exception ex)
			{
				EscalateError(ex);
			}
		}

		private void StopChild(InternalActorRef child)
		{
			//TODO: 
			ChildRestartInfo info;
			if(Children.TryGetByRef(child, out info))
			{
				UpdateChildrenCollection(children => children.IsAboutToTerminate(child));
				child.Stop();
			}
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


		private void ResumeActor(Exception cause)
		{
			if(_actor == null)
			{
				//TODO: Log "changing Resume into Create after " + cause
				CreateActorInstanceDueToFailure();
			}
			else if(_actor.IsCleared && cause != null)
			{
				//TODO: Log  "changing Resume into Restart after " + cause
				RecreateActor(cause);
			}
			else
			{
				var perpatrator = _failPerpatrator;
				try
				{
					ResumeThisOnly();
				}
				finally
				{
					if(cause != null) ClearFailedPerpatrator();
				}
				ResumeChildren(cause, perpatrator);
			}
		}


		private void HandleTerminateActor()
		{
			//TODO: UnwatchWatchedActors();

			//Stop all children
			Children.GetChildrenRefs().ForEach(c => c.Stop());

			var actorIsAlreadyTerminating = _actorStatus.IsTerminating;
			//If we have children we must wait for, then set status to Terminating, so we know what to do when all children have terminated
			var weHaveChildrenWeMustWaitFor = InterlockedSpin.ConditionallySwap(ref _actorStatus, _ => Children.HasChildrenThatAreTerminating(), _ => ActorStatus.Terminating);

			if(weHaveChildrenWeMustWaitFor)
			{
				if(!actorIsAlreadyTerminating)
				{
					//Children were not yet terminated, and this is the first time we try to terminate this actor
					//We should not not process normal messages while waiting for all children to terminate
					SuspendThisOnly();

					// do not propagate failures during shutdown to the supervisor
					SetFailedPerpatrator(this);
				}
			}
			else
			{
				SetTerminatedChildrenCollection();
				FinishTerminate();
			}
		}

		private void FinishTerminate()
		{
			var actor = _actor;
			try
			{
				if(actor != null) actor.PostStop();
			}
			catch(Exception)
			{
				//TODO: Log				
			}
			finally
			{
				try
				{
					var mailbox = _mailbox;
					_mailbox = System.DeadLettersMailbox;
					mailbox.DetachActor(this);
				}
				finally
				{
					try
					{
						_supervisor.SendSystemMessage(new ActorTerminated(this), this);
					}
					finally
					{
						try
						{
							TellWatchersWeDied();
						}
						finally
						{
							try
							{
								/* TODO: UnwatchWatchedActors(); */
							}
							finally
							{
								_actor.Clear();
								_actor = null;
							}
						}
					}
				}
			}
		}


		private void HandleActorTerminated(ActorTerminated message)
		{
			var terminatedActor = message.TerminatedActor;
			ChildRestartInfo childInfo;
			if(Children.TryGetByRef(terminatedActor, out childInfo))
			{
				//The terminated actor was a child. 
				//Remove it
				UpdateChildrenCollection(c => c.RemoveChild(terminatedActor));
				if(Children.IsEmpty)
				{
					//No more children.
					if(_actorStatus.IsTerminating)
					{
						FinishTerminate();
					}
					else if(_actorStatus.IsRecreating)
					{
						FinishRecreateActor(_actor, _actorStatus.RecreatingCause);
					}
					else if(_actorStatus.IsCreating)
					{
						FinishCreateActorInstanceDueToFailure();
					}
				}
			}
			if(_watching.Contains(terminatedActor))
			{
				_watching = _watching.Remove(terminatedActor);
				if(!_actorStatus.IsTerminating)
				{
					Send(new WatchedActorTerminated(terminatedActor), terminatedActor);
				}
			}
		}

		// Supervision -------------------------------------------------------------------------------------

		private void Supervise(InternalActorRef actor)
		{
			ChildInfo childInfo;
			var actorName = actor.Name;
			if(!_actorStatus.IsTerminating)
			{
				if(Children.TryGetByName(actorName, out childInfo))
				{
					var childRestartInfo = childInfo as ChildRestartInfo;
					if(childRestartInfo == null)
					{
						childRestartInfo = new ChildRestartInfo(actor);
						UpdateChildrenCollection(c => c.AddChild(actorName, childRestartInfo));
					}
				}
				else
				{
					//TODO: publish(Error(self.path.toString, clazz(actor), "received Supervise from unregistered child " + child + ", this will not end well"))
				}
			}
		}

		public void Watch(InternalActorRef actorToWatch)
		{
			if(actorToWatch != this && !_watching.Contains(actorToWatch))
			{
				actorToWatch.SendSystemMessage(new WatchActor(this, actorToWatch), this);
				_watching = _watching.Add(actorToWatch);
			}
		}

		public void Unwatch(InternalActorRef actor)
		{
			if(actor != this && _watching.Contains(actor))
			{
				actor.SendSystemMessage(new UnwatchActor(this, actor), this);
				_watching = _watching.Remove(actor);
			}
		}

		private void HandleWatch(WatchActor message)
		{
			var watcher = message.Watcher;
			if(watcher != this && !_watchedBy.Contains(watcher))
			{
				_watchedBy = _watchedBy.Add(watcher);
			}
		}

		private void HandleUnwatch(UnwatchActor message)
		{
			var watcher = message.Watcher;
			if(watcher != this && !_watchedBy.Contains(watcher))
			{
				_watchedBy = _watchedBy.Remove(watcher);
			}
		}

		private void TellWatchersWeDied()
		{
			foreach(var watcher in _watchedBy)
			{
				watcher.SendSystemMessage(new ActorTerminated(this), this);
			}
		}

		// Failure -------------------------------------------------------------------------------------


		protected virtual bool EscalateError(Exception exception, IEnumerable<ActorRef> childrenNotToSuspend = null) //virtual so we can override it to be able to write tests
		{
			if(!IsFailed)
			{
				try
				{
					SuspendThisOnly();
					var childrenToSkip = childrenNotToSuspend == null ? new HashSet<ActorRef>() : new HashSet<ActorRef>(childrenNotToSuspend);
					var currentMessage = CurrentMessage;
					var wasHandled =
						(currentMessage != null && PatternMatcher.Match<ActorFailed>(currentMessage.Message, m =>
						{
							SetFailedPerpatrator(m.Child);
							childrenToSkip.Add(m.Child);
						}))
						|| PatternMatcher.MatchAll(() => SetFailedPerpatrator(this));

					SuspendChildren(childrenToSkip);
					_supervisor.SendSystemMessage(new ActorFailed(this, exception, InstanceId), this);
				}
				catch(Exception e)
				{
					//TODO: Log
					try
					{
						Children.GetChildrenRefs().ForEach(c => c.Stop());
					}
					finally
					{
						//TODO: FinishTerminate()
					}
				}
			}
			return true;
		}

		private void SuspendChildren(ISet<ActorRef> childrenToSkip = null)
		{
			var childrenToSuspend = childrenToSkip.IsNullOrEmpty() ? Children.GetChildrenRefs() : Children.GetChildrenRefs().ExceptThoseInSet(childrenToSkip);
			childrenToSuspend.ForEach(a => a.Suspend());
		}

		void InternalActorRef.Suspend()
		{
			try
			{
				_mailbox.EnqueueSystemMessage(new SystemMessageEnvelope(this, SuspendActor.Instance, this));
			}
			catch(Exception e)
			{
				//TODO: Log
			}
		}

		private void ResumeChildren(Exception cause, ActorRef perpatrator)
		{
			Children.GetChildrenRefs().ForEach(c => c.Resume(c == perpatrator ? cause : null));
		}

		void InternalActorRef.Resume(Exception causedByFailure)
		{
			SafeEnqueueSystemMessage(new ResumeActor(causedByFailure));
		}

		void InternalActorRef.Restart(Exception causedByFailure)
		{
			SafeEnqueueSystemMessage(new RecreateActor(causedByFailure));
		}

		private void SafeEnqueueSystemMessage(SystemMessage message)
		{
			try
			{
				_mailbox.EnqueueSystemMessage(new SystemMessageEnvelope(this, message, this));
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
			var failedChild = (InternalActorRef)actorFailedMessage.Child;
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
				var cause = actorFailedMessage.CausedByFailure;
				if(!_actor.GetSupervisorStrategy().HandleFailure(childInfo, cause, Children.GetChildren().ToReadOnlyCollection()))
				{
					//It was not handle. Escalate by rethrowing the exception.
					EscalateError(cause);
				}
			}
		}


		private void SuspendThisOnly()
		{
			_mailbox.Suspend(this);
		}

		private void ResumeThisOnly()
		{
			_mailbox.Resume(this);
		}

		protected ChildrenCollection Children { get { return _childrenDoNotCallMeDirectly; } }

		private ActorRef _failPerpatrator = null;

		private bool IsFailed { get { return _failPerpatrator != null; } }

		private void SetFailedPerpatrator(ActorRef perpetrator)
		{
			_failPerpatrator = perpetrator;
		}

		private void ClearFailedPerpatrator()
		{
			_failPerpatrator = null;
		}

		// Children -------------------------------------------------------------------------------------

		private void ReserveChild(string name)
		{
			UpdateChildrenCollection(c => c.ReserveName(name));
		}

		private void ReleaseChild(string name)
		{
			UpdateChildrenCollection(c => c.ReleaseName(name));
		}

		private void UpdateChildrenCollection(Func<ChildrenCollection, ChildrenCollection> updater)
		{
#pragma warning disable 420		//Ok to disregard from CS0420 "a reference to a volatile field will not be treated as volatile" as we're using interlocked underneath, see http://msdn.microsoft.com/en-us/library/4bw5ewxy.aspx
			InterlockedSpin.Swap(ref _childrenDoNotCallMeDirectly, updater);
#pragma warning restore 420
		}

		private bool UpdateChildrenCollection(Func<ChildrenCollection, ChildrenCollection> updater, Predicate<ChildrenCollection> shouldUpdate)
		{
#pragma warning disable 420		//Ok to disregard from CS0420 "a reference to a volatile field will not be treated as volatile" as we're using interlocked underneath, see http://msdn.microsoft.com/en-us/library/4bw5ewxy.aspx
			return InterlockedSpin.ConditionallySwap(ref _childrenDoNotCallMeDirectly, c =>
			{
				if(shouldUpdate(c))
				{
					var newC = updater(c);
					return Tuple.Create(true, newC);
				}
				return Tuple.Create(false, c);
			});
#pragma warning restore 420
		}

		private void SetTerminatedChildrenCollection()
		{
			_childrenDoNotCallMeDirectly = TerminatedChildrenCollection.Instance;
		}

		/// <summary>If the message is of the specified type then the handler is invoked and true is returned. </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool IfMatchSys<T>(SystemMessage message, Action<T> handler) where T : class, SystemMessage
		{
			return PatternMatcher.Match(message, handler);
		}

		/// <summary>If the message is of the specified type then the handler is invoked and true is returned. </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool IfMatchSysCause<T>(SystemMessage message, Action<Exception> handler) where T : class, ExceptionSystemMessage
		{
			return PatternMatcher.Match<T>(message, m => handler(m.CausedByFailure));
		}

		private class ActorStatus
		{
			private static readonly ActorStatus _NormalInstance;
			private static readonly ActorStatus _TerminatingInstance;
			private static readonly ActorStatus _CreatingInstance;

			static ActorStatus()
			{
				_NormalInstance = new ActorStatus();
				_TerminatingInstance = new ActorStatus();
				_CreatingInstance = new ActorStatus();
			}

			protected ActorStatus()
			{
			}

			public bool IsNotCreatingRecreatingOrTerminating { get { return ReferenceEquals(this, _NormalInstance); } }
			public bool IsTerminating { get { return ReferenceEquals(this, _TerminatingInstance); } }
			public bool IsCreating { get { return ReferenceEquals(this, _CreatingInstance); } }
			public virtual bool IsRecreating { get { return false; } }
			public virtual Exception RecreatingCause { get { return null; } }

			public static ActorStatus Normal { get { return _NormalInstance; } }
			public static ActorStatus Terminating { get { return _TerminatingInstance; } }
			public static ActorStatus Creating { get { return _CreatingInstance; } }

			public static ActorStatus Recreating(Exception cause)
			{
				return new RecreationStatus(cause);
			}

			private class RecreationStatus : ActorStatus
			{
				private readonly Exception _cause;

				public RecreationStatus(Exception cause)
					: base()
				{
					_cause = cause;
				}

				public override bool IsRecreating { get { return true; } }
				public override Exception RecreatingCause { get { return _cause; } }
			}
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

		public void Start()
		{
			((InternalActorRef)this).Start();
		}
	}

	public class RootGuardianSupervisor : EmptyLocalActorRef
	{
		private const string _Name = "_Root-guardian-supervisor";
		public override string Name { get { return _Name; } }

		public RootGuardianSupervisor(RootActorPath root, ActorSystem system)
			: base(root / _Name, system)
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