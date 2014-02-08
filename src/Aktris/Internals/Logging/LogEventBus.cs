using System;
using Aktris.Events;

namespace Aktris.Internals.Logging
{
	public class LogEventBus : ActorEventBus<object, Type>
	{
		public bool Subscribe(ActorRef subscriber, Type to)
		{
			throw new NotImplementedException();
		}

		public bool Unsubscribe(ActorRef subscriber)
		{
			throw new NotImplementedException();
		}

		public bool Unsubscribe(ActorRef subscriber, Type to)
		{
			throw new NotImplementedException();
		}

		public void Publish(object @event)
		{
			throw new NotImplementedException();
		}
	}
}