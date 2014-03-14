using System;
using System.Threading.Tasks;

namespace Aktris.Dispatching
{
	public class ThreadPoolActionScheduler : IActionScheduler
	{

		public ThreadPoolActionScheduler()
		{
		}

		public void Schedule(Action action)
		{
			// "Task is now the preferred way to queue work to the thread pool.", Eric Eilebrecht http://blogs.msdn.com/b/ericeil/archive/2009/04/23/clr-4-0-threadpool-improvements-part-1.aspx
			Task.Run(action);
		}
	}
}