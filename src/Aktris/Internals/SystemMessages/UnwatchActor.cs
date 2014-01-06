using System.Diagnostics;

namespace Aktris.Internals.SystemMessages
{
	[DebuggerDisplay("Unwatch. Watcher={Watcher,nq}")]
	public class UnwatchActor : SystemMessage
	{
		private readonly InternalActorRef _watcher;

		public UnwatchActor(InternalActorRef watcher)
		{
			_watcher = watcher;
		}

		public InternalActorRef Watcher { get { return _watcher; } }
	}
}