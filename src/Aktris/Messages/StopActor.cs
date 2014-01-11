using Aktris.Internals;

namespace Aktris.Messages
{
	/// <summary>
	/// This will stop the actor after it has processed all messages sent prior to this message.
	/// Use <see cref="Actor.Stop()"/> if you want to stop the actor after the message currently being processed has completed.
	/// </summary>
	public class StopActor : AutoHandledMessage
	{
		private static readonly StopActor _Instance=new StopActor();
		private StopActor() {/* Intentionally left blank */}

		public static StopActor Instance { get { return _Instance; } }
	}
}