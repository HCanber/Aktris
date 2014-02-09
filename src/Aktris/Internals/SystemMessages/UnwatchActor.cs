using System.Diagnostics;

namespace Aktris.Internals.SystemMessages
{
	[DebuggerDisplay("Unwatch. Watcher={Watcher,nq}")]
	public class UnwatchActor : SystemMessage
	{
		private readonly InternalActorRef _watcher;
		private readonly InternalActorRef _watchee;

		public UnwatchActor(InternalActorRef watcher, InternalActorRef watchee)
		{
			_watcher = watcher;
			_watchee = watchee;
		}

		public InternalActorRef Watcher { get { return _watcher; } }

		public InternalActorRef Watchee { get { return _watchee; } }
	}
}