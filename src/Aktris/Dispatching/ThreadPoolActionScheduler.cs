using System;
using Fibrous;

namespace Aktris.Dispatching
{
	public class ThreadPoolActionScheduler : IActionScheduler
	{
		private readonly PoolFiber _fiber;

		public ThreadPoolActionScheduler()
		{
			_fiber = new PoolFiber();
			_fiber.Start();
		}

		public void Schedule(Action action)
		{
			_fiber.Enqueue(action);
		}
	}
}