using System;
using System.Threading.Tasks;
using Aktris.Exceptions;
using Aktris.Internals.Concurrency;

namespace Aktris
{
	public static class TaskExtensions
	{
		/// <summary>
		/// Returns <c>true</c> if the task has timed out, i.e. it is faulted with a <see cref="AskTimeoutException"/> 
		/// or a <see cref="TimeoutException">System.TimeoutException</see>.
		/// </summary>
		public static bool TimedOut(this Task task)
		{
			return task.ContainsException<AskTimeoutException, TimeoutException>();
		}
	}
}