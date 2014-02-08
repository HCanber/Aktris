namespace Aktris.Events
{
	public interface EventBus<in TEvent, in TClassifier, in TSubscriber> : EventBusSubscription<TClassifier, TSubscriber>, EventBusPublisher<TEvent> { }
}