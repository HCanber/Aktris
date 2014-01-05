namespace Aktris.Internals.SystemMessages
{
	public class WatchActor : SystemMessage
	{
		private readonly InternalActorRef _watcher;

		public WatchActor(InternalActorRef watcher)
		{
			_watcher = watcher;
		}

		public InternalActorRef Watcher { get { return _watcher; } }
	}
}