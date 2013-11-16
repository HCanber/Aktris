using System;

namespace Aktris.Dispatching
{
	public interface IScheduler
	{
		void Schedule(Action action);
	}
}