﻿using System.Diagnostics;

namespace Aktris.Internals.SystemMessages
{
	[DebuggerDisplay("Watch. Watcher={Watcher,nq}")]
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