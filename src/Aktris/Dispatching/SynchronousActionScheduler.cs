using System;

namespace Aktris.Dispatching
{
	public class SynchronousActionScheduler : IActionScheduler
	{
		public void Schedule(Action action)
		{
			action();
		}
	}
}