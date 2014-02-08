namespace Aktris.Events
{
	public interface ActorEventBus<in TEvent, in TClassifier> : EventBusSubscription<TClassifier, ActorRef>, EventBusPublisher<TEvent> { }
}