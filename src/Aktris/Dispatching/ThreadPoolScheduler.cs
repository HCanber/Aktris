using System;
using Fibrous;

namespace Aktris.Dispatching
{
	public class ThreadPoolScheduler : IScheduler
	{
		private readonly PoolFiber _fiber;

		public ThreadPoolScheduler()
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