using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text.RegularExpressions;
using Aktris.Exceptions;
using Aktris.Internals;
using Aktris.JetBrainsAnnotations;

namespace Aktris
{
	public abstract class Actor : IActorCreator
	{
		private bool _hasBeenInitialized;
		private MessageHandlerConfigurator _constructorMessageHandlerConfigurator;
		private MessageHandler _defaultMessageHandler;
		private LocalActorRef _self;
		private readonly ActorSystem _system;
		private LocalActorRefFactory _localActorRefFactory;


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
			_constructorMessageHandlerConfigurator = new MessageHandlerConfigurator();
			_localActorRefFactory = _system.LocalActorRefFactory;
		}

		/// <summary>
		/// This one is used for internal testing only
		/// </summary>
		internal Actor(LocalActorRef actorRef, ActorSystem system, LocalActorRefFactory localActorRefFactory)
		{
			if(actorRef==null)
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


		protected internal virtual void PreStart()
		{
			//Intentionally left blank
		}


		/// <summary>
		/// Handles a message. By default it calls the handlers registered in the constructor using any of the Receive methods.
		/// If the actor has handled the message it should return <c>true</c>; otherwise <c>false</c>.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <returns><c>true</c> if the actor has handled the message; <c>false</c> otherwise.</returns>
		internal protected virtual bool HandleMessage(object message)
		{
			return _defaultMessageHandler(message,Sender);
		}


		public ActorRef CreateActor(ActorCreationProperties actorCreationProperties, string name = null)
		{
			return _self.CreateActor(actorCreationProperties, name);
		}

		// API for the user that implements an actor to define message handlers -----------------------------------------

		/// <summary>
		/// Registers a handler for incoming messages of the specified type 
		/// <typeparamref name="T"/>.
		/// <remarks>This may only be called from the constructor.</remarks>
		/// <remarks>Note that handlers registered prior to this may have handled the message already. 
		/// In that case, this handler will not be invoked.</remarks>
		/// </summary>
		/// <typeparam name="T">The type of the message</typeparam>
		/// <param name="handler">The message handler that is invoked for incoming messages of the specified type <typeparamref name="T"/></param>
		protected void Receive<T>([NotNull] Action<T> handler)
		{
			EnsureMayConfigureMessageHandlers();
			_constructorMessageHandlerConfigurator.Receive<T>(handler);
		}

		/// <summary>
		/// Swallows all incomming, unhandled messages.
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

		protected void AddReceiver(Type type, Action<object> handler)
		{
			EnsureMayConfigureMessageHandlers();
			_constructorMessageHandlerConfigurator.AddReceiver(type, handler);
		}
		protected void AddReceiver(Type type, Func<object,bool> handler)
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

		// ReSharper disable once VirtualMemberNeverOverriden.Global   Init is virtual in order to be mockable
		internal virtual void Init()
		{
			_defaultMessageHandler = _constructorMessageHandlerConfigurator.CreateMessageHandler();
			_constructorMessageHandlerConfigurator = null;
			_hasBeenInitialized = true;
		}


	}
}