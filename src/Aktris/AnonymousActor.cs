using System;

namespace Aktris
{
	public class AnonymousActor : Actor
	{
		private AnonymousActor(MessageHandlerConfigurator messageHandlerConfigurator)
		{
			CopyFrom(messageHandlerConfigurator);
		}

		public static Actor Create(Action<MessageHandlerConfigurator> messageHandlersConfiguration)
		{
			var configurator = new MessageHandlerConfigurator();
			messageHandlersConfiguration(configurator);
			return new AnonymousActor(configurator);
		}


		public static Actor Create<TMessage>(Action<TMessage> handler)
		{
			var configurator = new MessageHandlerConfigurator();
			configurator.AddReceiver(typeof(TMessage), (message, sender) => handler((TMessage)message));
			return new AnonymousActor(configurator);
		}
		public static Actor Create<TMessage>(MessageHandler<TMessage> handler)
		{
			var configurator = new MessageHandlerConfigurator();
			configurator.AddReceiver(typeof(TMessage),(message, sender) => handler((TMessage) message,sender)	);
			return new AnonymousActor(configurator);
		}
	}
}