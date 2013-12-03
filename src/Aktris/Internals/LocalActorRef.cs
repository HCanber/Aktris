using System;
using System.Collections.Concurrent;
using System.Diagnostics;
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
		private Actor _actor;
		private readonly SenderActorRef _deadLetterSender;
		private Envelope _currentMessage;
		private volatile ChildrenCollection _children = EmptyChildrenCollection.Instance;


		public LocalActorRef([NotNull] ActorSystem system, [NotNull] ActorInstantiator actorInstantiator, [NotNull] ActorPath path, [NotNull] Mailbox mailbox)
		{
			if(system == null) throw new ArgumentNullException("system");
			if(actorInstantiator == null) throw new ArgumentNullException("actorInstantiator");
			if(path == null) throw new ArgumentNullException("path");
			if(mailbox == null) throw new ArgumentNullException("mailbox");
			_system = system;
			_actorInstantiator = actorInstantiator;
			_path = path;
			_mailbox = mailbox;
			mailbox.EnqueueSystemMessage(new SystemMessageEnvelope(this, new CreateActor(), this));
			_deadLetterSender = new SenderActorRef(system.DeadLetters, this);
		}

		public string Name { get { return _path.Name; } }
		public ActorPath Path { get { return _path; } }
		public uint InstanceId { get { return _path.InstanceId; } }

		public ActorSystem System { get { return _system; } }

		protected Mailbox Mailbox { get { return _mailbox; } }

		public void Start()
		{
			_mailbox.SetActor(this);
		}

		public void Send(object message, ActorRef sender)
		{
			sender = UnwrapSenderActorRef(sender);
			var envelope = new Envelope(this, message, sender ?? _system.DeadLetters);
			_mailbox.Enqueue(envelope);
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
				IfMatchSys<CreateActor>(message, CreateActorInstance);
			if(!wasMatched)
			{
				//This should never happen. If it does a new SystemMessage type has been added
				throw new InvalidOperationException(string.Format("Unexpected message type: {0}. Message was: {1}", message.GetType(), message));
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
				throw new ActorInitializationException(this, "An error occured while creating the actor. See inner exception", ex);
			}
			finally
			{
				LocalActorRefStack.PopActorAndMarkerFromStack();
			}
		}

		private Actor NewActorInstance()
		{
			var actor = _actorInstantiator.CreateNewActor();
			if(actor == null) throw new ActorInitializationException(this, "CreateNewActor returned null");
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
			InternalActorRef actorRef;
			try
			{
				ReserveChild(name);
				var instanceId = CreateInstanceId();
				var path = new ChildActorPath(new RootActorPath("FAKE will be fixed with supervisors"), name, instanceId);	//TODO
				actorRef = _system.LocalActorRefFactory.CreateActor(_system, actorCreationProperties, path);
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

		private void ReserveChild(string name)
		{
			SwapChildrenCollection(c => c.ReserveName(name));
		}

		private void ReleaseChild(string name)
		{
			SwapChildrenCollection(c => c.ReleaseName(name));
		}

		private ChildrenCollection SwapChildrenCollection(Func<ChildrenCollection, ChildrenCollection> updater)
		{
#pragma warning disable 420		//Ok to disregard from CS0420 "a reference to a volatile field will not be treated as volatile" as we're using interlocked underneath, see http://msdn.microsoft.com/en-us/library/4bw5ewxy.aspx
			return InterlockedSpin.Swap(ref _children, updater);
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
		public GuardianActorRef([NotNull] ActorSystem system, [NotNull] ActorInstantiator actorInstantiator, [NotNull] ActorPath path, [NotNull] Mailbox mailbox)
			: base(system, actorInstantiator, path, mailbox)
		{
		}

		public InternalActorRef CreateGuardian(Func<Actor> actorFactory, string name)
		{
			var props = new DelegateActorCreationProperties(actorFactory);
			var guardian = CreateLocalActorReference(props, name);
			return guardian;
		}
	}
}