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
	}
}