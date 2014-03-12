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

		public static bool ContainsException<T1, T2>(this Task task)
			where T1 : Exception
			where T2 : Exception
		{
			return !task.IsFaulted ? false : task.Exception.Flatten().InnerExceptions.Any(e => e is T1 || e is T2);
		}

		public static bool ContainsException<T1, T2, T3>(this Task task)
			where T1 : Exception
			where T2 : Exception
			where T3 : Exception
		{
			return !task.IsFaulted ? false : task.Exception.Flatten().InnerExceptions.Any(e => e is T1 || e is T2 || e is T3);
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

		public static Task<T> CreateCompletedTask<T>(T result)
		{
			var taskCompletionSource = new TaskCompletionSource<T>();
			taskCompletionSource.SetResult(result);
			return taskCompletionSource.Task;
		}

		public static Task CreateCompletedTask()
		{
			var taskCompletionSource = new TaskCompletionSource<VoidTypeStruct>();
			taskCompletionSource.SetResult(default(VoidTypeStruct));
			return taskCompletionSource.Task;
		}

		public static Task CreateInfiniteTask()
		{
			return CreateInfiniteTask<VoidTypeStruct>();
		}

		public static Task CreateInfiniteTask<T>()
		{
			var taskCompletionSource = new TaskCompletionSource<VoidTypeStruct>();
			return taskCompletionSource.Task;
		}

		internal struct VoidTypeStruct { }
	}
}