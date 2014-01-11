using System.Diagnostics;

namespace Aktris.Internals.SystemMessages
{
	[DebuggerDisplay("Watch. Watcher={Watcher,nq}")]
	public class WatchActor : SystemMessage
	{
		private readonly InternalActorRef _watcher;
		private readonly InternalActorRef _watchee;

		public WatchActor(InternalActorRef watcher, InternalActorRef watchee)
		{
			_watcher = watcher;
			_watchee = watchee;
		}

		public InternalActorRef Watcher { get { return _watcher; } }

		public InternalActorRef Watchee { get { return _watchee; } }
	}
}