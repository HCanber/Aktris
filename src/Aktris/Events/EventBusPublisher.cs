namespace Aktris.Events
{
	public interface EventBusPublisher<in TEvent>
	{
		/// <summary> Publishes the specified event to this bus.</summary>
		void Publish(object @event);
	}
}