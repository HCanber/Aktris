using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Remoting.Contexts;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Aktris.Exceptions;
using Aktris.Internals;
using Aktris.Internals.Concurrency;
using Aktris.Internals.Helpers;
using Aktris.Internals.Logging;
using Aktris.JetBrainsAnnotations;
using Aktris.Logging;
using Aktris.Supervision;

namespace Aktris
{
	public abstract class Actor : IActorCreator
	{
		private bool _hasBeenInitialized;
		private bool _mayConfigureHandlers;
		private MessageHandlerConfigurator _constructorMessageHandlerConfigurator;
		private MessageHandler _defaultMessageHandler;
		private InternalActorRef _self;
		private readonly ActorSystem _system;
		private LocalActorRefFactory _localActorRefFactory;
		private ILogger _logger;

		protected Actor()
		{
			LocalActorRef actorRef;
			if(!LocalActorRefStack.TryGetActorRefFromStack(out actorRef))
			{
				throw new InvalidOperationException(StringFormat.SafeFormat("Cannot create a new instance of type {0} directly using new(). An actor can only be created via the CreateActor methods.", GetType().FullName));
			}
			LocalActorRefStack.MarkActorRefConsumedInStack();
			_system = actorRef.System;
			_self = actorRef;
			PrepareForConfiguringMessageHandler();
			_localActorRefFactory = _system.LocalActorRefFactory;
		}



		// ReSharper disable once VirtualMemberNeverOverriden.Global   Init is virtual in order to be mockable
		internal virtual void Init(LocalActorRef self)
		{
			//This might be called directly after the constructor, or when the same actor instance has been returned
			//during recreate. In the second case, _self has been set to DeadLetter so we need to set it again.
			_self = self;
			if(!_hasBeenInitialized)	//Do not perform this when "recreating" the same instance
			{
				_defaultMessageHandler = BuildNewHandler();
				_constructorMessageHandlerConfigurator = null;
				_hasBeenInitialized = true;
			}
		}



		/// <summary>
		/// This one is used for internal testing only
		/// </summary>
		internal Actor(LocalActorRef actorRef, ActorSystem system, LocalActorRefFactory localActorRefFactory)
		{
			if(actorRef == null)
			{
				if(!LocalActorRefStack.TryGetActorRefFromStack(out actorRef))
				{
					throw new InvalidOperationException(StringFormat.SafeFormat("Cannot create a new instance of type {0} directly using new(). An actor can only be created via the CreateActor methods.", GetType().FullName));
				}
				LocalActorRefStack.MarkActorRefConsumedInStack();
			}
			_system = system;
			_self = actorRef;
			_constructorMessageHandlerConfigurator = new MessageHandlerConfigurator();
			_localActorRefFactory = localActorRefFactory;
		}

		/// <summary>The actor that sent the last message.</summary>
		[NotNull]
		protected internal SenderActorRef Sender { get; internal set; }

		/// <summary>The reference to this actor</summary>
		[NotNull]
		protected internal ActorRef Self { get { return _self; } }
		[NotNull]
		internal InternalActorRef InternalSelf { get { return _self; } }

		internal bool IsCleared { get { return _self == _system.DeadLetters; } }

		protected virtual SupervisorStrategy SupervisorStrategy { get { return SupervisorStrategy.DefaultStrategy; } }

		internal SupervisorStrategy GetSupervisorStrategy() { return SupervisorStrategy ?? SupervisorStrategy.DefaultStrategy; }

		protected ILogger Log { get { return _logger ?? (_logger = new LoggingAdapter(_system.EventStream, this)); } }

		protected ActorRef Parent { get { return _self.Parent; } }

		protected IScheduler Scheduler { get { return _system.Scheduler; } }

		protected internal virtual void PreFirstStart() {/*Intentionally left blank*/}
		protected internal virtual void PreStart() {/*Intentionally left blank*/}
		protected internal virtual void PostStop() {/*Intentionally left blank*/}
		protected internal virtual void PreRestart(Exception cause, object message)
		{
			InternalSelf.UnwatchAndStopChildren();
		}
		protected internal virtual void PostRestart(Exception cause) {/*Intentionally left blank*/}

		/// <summary>
		/// Handles a message. By default it calls the handlers registered in the constructor using any of the Receive methods.
		/// If the actor has handled the message it should return <c>true</c>; otherwise <c>false</c>.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="sender"></param>
		/// <returns><c>true</c> if the actor has handled the message; <c>false</c> otherwise.</returns>
		internal protected virtual bool HandleMessage(object message, SenderActorRef sender)
		{
			return _defaultMessageHandler(message, Sender);
		}

		protected ActorRef CreateActor<T>(string name = null) where T : Actor, new()
		{
			return _self.CreateActor(ActorCreationProperties.Create<T>(), name);
		}

		ActorRef IActorCreator.CreateActor(ActorCreationProperties actorCreationProperties, string name = null)
		{
			return CreateActor(actorCreationProperties, name);
		}

		protected ActorRef CreateActor(ActorCreationProperties actorCreationProperties, string name = null)
		{
			return _self.CreateActor(actorCreationProperties, name);
		}

		protected ActorRef CreateActor<T>(Func<T> creator, string name = null) where T : Actor
		{
			return _self.CreateActor(ActorCreationProperties.Create(creator), name);
		}

		protected void Watch(ActorRef actorToWatch)
		{
			_self.Watch((InternalActorRef)actorToWatch);
		}

		protected void Unwatch(ActorRef actorToWatch)
		{
			_self.Unwatch((InternalActorRef)actorToWatch);
		}

		protected void Stop(ActorRef actorToImmediatelyStop)
		{
			((InternalActorRef)actorToImmediatelyStop).Stop();
		}
		protected void Stop() { _self.Stop(); }

		// API for the user that implements an actor to define message handlers -----------------------------------------

		/// <summary>
		/// Registers a handler for incoming messages of the specified type 
		/// If <paramref name="matches"/>!=<c>null</c> then this must return true before a message is passed to <paramref name="handler"/>.
		/// <typeparamref name="T"/>.
		/// <remarks>This may only be called from the constructor.</remarks>
		/// <remarks>Note that handlers registered prior to this may have handled the message already. 
		/// In that case, this handler will not be invoked.</remarks>
		/// </summary>
		/// <typeparam name="T">The type of the message</typeparam>
		/// <param name="handler">The message handler that is invoked for incoming messages of the specified type <typeparamref name="T"/></param>
		/// <param name="matches">When not <c>null</c> it is used to determine if the message matches.</param>
		protected void Receive<T>([NotNull] Action<T> handler, Predicate<T> matches = null)
		{
			EnsureMayConfigureMessageHandlers();
			_constructorMessageHandlerConfigurator.Receive<T>(handler, matches);
		}

		/// <summary>
		/// Swallows all incoming, unhandled messages.
		/// <remarks>This may only be called from the constructor.</remarks>
		/// <remarks>Note that handlers registered prior to this may have handled the message already. 
		/// In that case, this handler will not be invoked.</remarks>
		/// </summary>
		protected void ReceiveAny()
		{
			ReceiveAny(_ => { });
		}

		/// <summary>
		/// Registers a handler for incoming messages of any type.
		/// <remarks>This may only be called from the constructor.</remarks>
		/// <remarks>Note that handlers registered prior to this may have handled the message already. 
		/// In that case, this handler will not be invoked.</remarks>
		/// </summary>
		/// <param name="handler">The message handler that is invoked for all</param>
		protected void ReceiveAny([NotNull] Action<object> handler)
		{
			EnsureMayConfigureMessageHandlers();
			_constructorMessageHandlerConfigurator.ReceiveAny(handler);
		}

		/// <summary>
		/// Forwards all incoming messages oto the specified actor.
		/// <remarks>This may only be called from the constructor.</remarks>
		/// <remarks>Note that handlers registered prior to this may have handled the message already. 
		/// In that case, this handler will not be invoked.</remarks>
		/// </summary>
		/// <param name="receiver">The recipient of all incoming messages</param>
		protected void ReceiveAnyAndForward(ActorRef receiver)
		{
			EnsureMayConfigureMessageHandlers();
			_constructorMessageHandlerConfigurator.ReceiveAnyAndForward(receiver);
		}

		protected void AddReceiver(Type type, Action<object> handler)
		{
			EnsureMayConfigureMessageHandlers();
			_constructorMessageHandlerConfigurator.AddReceiver(type, handler);
		}
		protected void AddReceiver(Type type, Func<object, bool> handler)
		{
			EnsureMayConfigureMessageHandlers();
			_constructorMessageHandlerConfigurator.AddReceiver(type, handler);
		}

		protected void AddReceiver(Type type, MessageHandler handler)
		{
			EnsureMayConfigureMessageHandlers();
			_constructorMessageHandlerConfigurator.AddReceiver(type, handler);
		}

		internal void CopyFrom(MessageHandlerConfigurator other)
		{
			EnsureMayConfigureMessageHandlers();
			_constructorMessageHandlerConfigurator.CopyFrom(other);
		}

		private void EnsureMayConfigureMessageHandlers()
		{
			if(_hasBeenInitialized) throw new InvalidOperationException("You may only call Receive-methods from the constructor.");
		}


		private void PrepareForConfiguringMessageHandler()
		{
			if(_mayConfigureHandlers) throw new InvalidOperationException("Already configuring message handlers");
			_constructorMessageHandlerConfigurator = new MessageHandlerConfigurator();
			_mayConfigureHandlers = true;
		}

		private MessageHandler BuildNewHandler()
		{
			EnsureMayConfigureMessageHandlers();
			var newHandler = _constructorMessageHandlerConfigurator.CreateMessageHandler();
			_mayConfigureHandlers = false;
			return newHandler;
		}

		public void Become(Action configure, bool discardOld=true)
		{
			PrepareForConfiguringMessageHandler();
			try
			{
				configure();
				var newHandler=BuildNewHandler();
				InternalSelf.Become(newHandler,discardOld);
			}
			finally
			{
				_mayConfigureHandlers = false;
			}
		}

		public void Become(MessageHandler newHandler, bool discardOld = true)
		{
			InternalSelf.Become(newHandler, discardOld);
		}

		public void Unbecome()
		{
			InternalSelf.Unbecome();
		}




		internal void Clear()
		{
			_self = (InternalActorRef)_system.DeadLetters;
		}


	}
}