using System;

namespace Aktris.Dispatching
{
	public interface IActionScheduler
	{
		void Schedule(Action action);
	}
}