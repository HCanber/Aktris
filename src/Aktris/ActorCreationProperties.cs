using System;
using Aktris.Dispatching;

namespace Aktris
{
	public abstract class ActorCreationProperties : ActorInstantiator
	{
		public abstract Actor CreateNewActor();


		public static ActorCreationProperties Create<T>() where T : Actor, new()
		{
			//TODO: Create this thru the ioc container instead of Activator so that dependencies can be injected. Then remove:  T: new()
			return new DelegateActorCreationProperties(() => new T());
		}

		public static ActorCreationProperties Create<T>(Func<T> creator) where T : Actor
		{
			//TODO: Create this thru the ioc container instead of Activator so that dependencies can be injected.
			return new DelegateActorCreationProperties(creator);
		}

		public static ActorCreationProperties CreateAnonymous(Action<MessageHandlerConfigurator> messageHandlersConfiguration)
		{
			return new DelegateActorCreationProperties(() => AnonymousActor.Create(messageHandlersConfiguration));
		}
		public static ActorCreationProperties CreateAnonymous<TMessage>(Action<TMessage> handler)
		{
			return new DelegateActorCreationProperties(() => AnonymousActor.Create(handler));
		}
		public static ActorCreationProperties CreateAnonymous<TMessage>(MessageHandler<TMessage> handler)
		{
			return new DelegateActorCreationProperties(() => AnonymousActor.Create(handler));
		}

		public abstract Mailbox CreateMailbox();
	}
}