using System;
using System.Threading;
using System.Threading.Tasks;

namespace Aktris.Internals.Concurrency
{
	public static class TaskTimeoutExtensions
	{
		//Code copied from http://blogs.msdn.com/b/pfxteam/archive/2011/11/10/10235834.aspx



		//There is no non-generic version of TaskCompletionSource<TResult>. So, if
		//you want a completion source for a Task (as opposed to a Task<TResult>), 
		//you still need to provide some throwaway TResult type to TaskCompletionSource. 
		//For this example, we’ve created a dummy type (VoidTypeStruct), and we create a 
		//TaskCompletionSource<VoidTypeStruct>.
		internal struct VoidTypeStruct { }

		/// <summary>
		/// Returns a task that either completes, by completing <paramref name="task"/> within the specified timeout, 
		/// or fails with a <see cref="TimeoutException"/>
		/// </summary>
		public static Task TimeoutAfter(this Task task, TimeSpan timeSpan)
		{
			return TimeoutAfter(task, (long)timeSpan.TotalMilliseconds);
		}

		/// <summary>
		/// Returns a task that either completes, by completing <paramref name="task"/> within the specified timeout, 
		/// or fails with a <see cref="TimeoutException"/>
		/// </summary>
		public static Task TimeoutAfter(this Task task, long millisecondsTimeout)
		{
			// Short-circuit #1: infinite timeout or task already completed
			if(task.IsCompleted || (millisecondsTimeout == Timeout.Infinite))
			{
				// Either the task has already completed or timeout will never occur.
				// No proxy necessary.
				return task;
			}

			// tcs.Task will be returned as a proxy to the caller
			var tcs = new TaskCompletionSource<VoidTypeStruct>();

			// Short-circuit #2: zero timeout
			if(millisecondsTimeout <= 0)
			{
				// We've already timed out.
				tcs.SetException(new TimeoutException());
				return tcs.Task;
			}

			// Set up a timer to complete after the specified timeout period
			var timer = new Timer(state =>
			{
				// Recover your state information
				var myTcs = (TaskCompletionSource<VoidTypeStruct>)state;

				// Fault our proxy with a TimeoutException
				myTcs.TrySetException(new TimeoutException());
			}, tcs, millisecondsTimeout, Timeout.Infinite);

			// Wire up the logic for what happens when source task completes
			task.ContinueWith((antecedent, state) =>
			{
				// Recover our state data
				var tuple = (Tuple<Timer, TaskCompletionSource<VoidTypeStruct>>)state;

				// Cancel the Timer
				tuple.Item1.Dispose();

				// Marshal results to proxy
				MarshalTaskResults(antecedent, tuple.Item2);
			},
			Tuple.Create(timer, tcs),
			CancellationToken.None,
			TaskContinuationOptions.ExecuteSynchronously,
			TaskScheduler.Default);

			return tcs.Task;
		}



		/// <summary>
		/// Returns a task that either completes, by completing <paramref name="task"/> within the specified timeout, 
		/// or fails with a <see cref="TimeoutException"/>
		/// </summary>
		public static Task<T> TimeoutAfter<T>(this Task<T> task, TimeSpan timeSpan)
		{
			return TimeoutAfter(task, (long)timeSpan.TotalMilliseconds);
		}

		/// <summary>
		/// Returns a task that either completes, by completing <paramref name="task"/> within the specified timeout, 
		/// or fails with a <see cref="TimeoutException"/>
		/// </summary>
		public static Task<T> TimeoutAfter<T>(this Task<T> task, long millisecondsTimeout)
		{
			// Short-circuit #1: infinite timeout or task already completed
			if(task.IsCompleted || (millisecondsTimeout == Timeout.Infinite))
			{
				// Either the task has already completed or timeout will never occur.
				// No proxy necessary.
				return task;
			}

			// tcs.Task will be returned as a proxy to the caller
			var tcs = new TaskCompletionSource<T>();

			// Short-circuit #2: zero timeout
			if(millisecondsTimeout <= 0)
			{
				// We've already timed out.
				tcs.SetException(new TimeoutException());
				return tcs.Task;
			}

			// Set up a timer to complete after the specified timeout period
			var timer = new Timer(state =>
			{
				// Recover your state information
				var myTcs = (TaskCompletionSource<VoidTypeStruct>)state;

				// Fault our proxy with a TimeoutException
				myTcs.TrySetException(new TimeoutException());
			}, tcs, millisecondsTimeout, Timeout.Infinite);

			// Wire up the logic for what happens when source task completes
			task.ContinueWith((antecedent, state) =>
			{
				// Recover our state data
				var tuple = (Tuple<Timer, TaskCompletionSource<VoidTypeStruct>>)state;

				// Cancel the Timer
				tuple.Item1.Dispose();

				// Marshal results to proxy
				MarshalTaskResults(antecedent, tuple.Item2);
			},
			Tuple.Create(timer, tcs),
			CancellationToken.None,
			TaskContinuationOptions.ExecuteSynchronously,
			TaskScheduler.Default);

			return tcs.Task;
		}


		private static void MarshalTaskResults<TResult>(Task source, TaskCompletionSource<TResult> proxy)
		{
			switch(source.Status)
			{
				case TaskStatus.Faulted:
					proxy.TrySetException(source.Exception);
					break;
				case TaskStatus.Canceled:
					proxy.TrySetCanceled();
					break;
				case TaskStatus.RanToCompletion:
					var castedSource = source as Task<TResult>;
					proxy.TrySetResult(
							castedSource == null ? default(TResult) : // source is a Task
									castedSource.Result); // source is a Task<TResult>
					break;
			}
		}
	}
}