using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using Aktris.Dispatching;
using Aktris.Exceptions;
using Aktris.Internals.SystemMessages;
using Aktris.JetBrainsAnnotations;

namespace Aktris.Internals
{
	public class LocalActorRef : ILocalActorRef
	{
		private readonly ActorSystem _system;
		private readonly ActorInstantiator _actorInstantiator;
		private readonly string _name;
		private readonly Mailbox _mailbox;
		private Actor _actor;
		private readonly SenderActorRef _deadLetterSender;
		private Envelope _currentMessage;

		public LocalActorRef([NotNull] ActorSystem system, [NotNull] ActorInstantiator actorInstantiator, [NotNull] string name, [NotNull] Mailbox mailbox)
		{
			if(system == null) throw new ArgumentNullException("system");
			if(actorInstantiator == null) throw new ArgumentNullException("actorInstantiator");
			if(name == null) throw new ArgumentNullException("name");
			if(mailbox == null) throw new ArgumentNullException("mailbox");
			_system = system;
			_actorInstantiator = actorInstantiator;
			_name = name;
			_mailbox = mailbox;
			mailbox.EnqueueSystemMessage(new SystemMessageEnvelope(this, new CreateActor(), this));
			_deadLetterSender = new SenderActorRef(system.DeadLetters, this);
		}

		public string Name { get { return _name; } }

		public ActorSystem System { get { return _system; } }

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
				_actor.Sender = new SenderActorRef(envelope.Sender,this);
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
				IfMatchSys<CreateActor>(message, CreateActor);
			if(!wasMatched)
			{
				//This should never happen. If it does a new SystemMessage type has been added
				throw new InvalidOperationException(string.Format("Unexpected message type: {0}. Message was: {1}", message.GetType(), message));
			}
		}

		private void CreateActor(CreateActor obj)
		{
			try
			{
				LocalActorRefStack.PushActorRefToStack(this);
				var actor = NewActor();
				actor.Init();
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

		private Actor NewActor()
		{
			var actor = _actorInstantiator.CreateNewActor();
			if(actor == null) throw new ActorInitializationException(this, "CreateNewActor returned null");
			return actor;
		}

		public ActorRef CreateActor(ActorCreationProperties actorCreationProperties, string name = null)
		{
			if(name != null)
			{
				ActorNameValidator.EnsureNameIsValid(name);
			}
			else name = _system.UniqueNameCreator.GetNextRandomName();
			var actorRef = _system.LocalActorRefFactory.CreateActor(_system, actorCreationProperties, name);
			actorRef.Start();
			return actorRef;
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
		public GuardianActorRef([NotNull] ActorSystem system, [NotNull] ActorInstantiator actorInstantiator, [NotNull] string name, [NotNull] Mailbox mailbox) : base(system, actorInstantiator, name, mailbox)
		{
		}


	}
}