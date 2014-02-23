using System;
using System.Linq;
using System.Threading.Tasks;

namespace Aktris.Internals.Concurrency
{
	public static class TaskExt
	{
		public static Exception GetFirstException(this Task task)
		{
			return !task.IsFaulted ? null : task.Exception.Flatten().InnerExceptions.FirstOrDefault();
		}
		public static Exception GetFirstException(this AggregateException exception)
		{
			return exception == null ? null : exception.Flatten().InnerExceptions.FirstOrDefault();
		}

		public static bool ContainsException<TException>(this Task task) where TException : Exception
		{
			return !task.IsFaulted ? false : task.Exception.Flatten().InnerExceptions.Any(e => e is TException);
		}

		public static bool ContainsException<TException>(this AggregateException exception) where TException : Exception
		{
			return exception != null && exception.Flatten().InnerExceptions.Any(e => e is TException);
		}

		public static Task<T> CreateFailedTask<T>(Exception exception)
		{
			var taskCompletionSource = new TaskCompletionSource<T>();
			taskCompletionSource.SetException(exception);
			return taskCompletionSource.Task;
		}
	}
}