using System;
using Aktris.JetBrainsAnnotations;

namespace Aktris.Internals.Logging
{
	public class EventStream : LoggingEventBus
	{
		private readonly ActorRef _deadLetterActor;
		private readonly ActorSystem _system;
		private readonly bool _debug;
		//TODO: Add SubchannelClassification, which means that if you listen to a Class, you'll receive any message that is of that type or a subtype.
		private readonly LookupClassification<object, Type, ActorRef> _subscriptionHandler;

		public EventStream([NotNull] ActorSystem system, bool debug = false)
			: base(system)
		{
			_deadLetterActor = system.DeadLetters;
			_system = system;
			_debug = debug;
			_subscriptionHandler = new LookupClassification<object, Type, ActorRef>(Classify, Publish);
		}

		private static Type Classify(object arg)
		{
			return arg.GetType();
		}

		private void DebugLog(string message)
		{
			if(_debug)
				Publish(new DebugLogEvent(GetType().Name, GetType(), message));
		}

		public bool Subscribe<T>([NotNull] ActorRef subscriber)
		{
			return Subscribe(subscriber, typeof(T));
		}

		public override bool Subscribe([NotNull] ActorRef subscriber, Type to)
		{
			if(subscriber == null) throw new ArgumentNullException("subscriber");
			DebugLog("Subscribing "+ subscriber+" to channel " + to);
			return	_subscriptionHandler.Subscribe(subscriber, to);
		}

		public override bool Unsubscribe(ActorRef subscriber)
		{
			if(subscriber == null) throw new ArgumentNullException("subscriber");
			var unsubscribed = _subscriptionHandler.Unsubscribe(subscriber);
			DebugLog("Unsubscribing " + subscriber + " from all channels");
			return unsubscribed;
		}

		public bool Unsubscribe<T>([NotNull] ActorRef subscriber)
		{
			return Unsubscribe(subscriber, typeof(T));
		}

		public override bool Unsubscribe([NotNull] ActorRef subscriber, Type to)
		{
			if(subscriber == null) throw new ArgumentNullException("subscriber");
			var unsubscribed = _subscriptionHandler.Unsubscribe(subscriber, to);
			DebugLog("Unsubscribing " + subscriber + " from channel " + to);
			return unsubscribed;
		}

		public override void Publish(object @event)
		{
		_subscriptionHandler.Publish(@event);
		}

		private void Publish(object @event, ActorRef subscriber)
		{
			var actorRef = (InternalActorRef)subscriber;
			if(actorRef.IsTerminated)
				Unsubscribe(subscriber);
			else
				subscriber.Send(@event, null);
		}
	}
}