using System;

namespace Aktris.Dispatching
{
	public class SynchronousScheduler : IScheduler
	{
		public void Schedule(Action action)
		{
			action();
		}
	}
}