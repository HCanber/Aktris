using System;
using System.Linq;
using System.Threading.Tasks;
using Aktris.Internals.Concurrency;

namespace Aktris.Test.TestHelpers
{
	public static class TaskExtensions
	{
		public static Task Then<TResult>(this Task<TResult> task, Action<Task<TResult>> onSuccess, Action<Exception> onException, Action onCancel=null, Action finalAction=null)
		{
			if(task == null) throw new ArgumentNullException("task");
			if(onSuccess == null) throw new ArgumentNullException("onSuccess");

			var tcs = new TaskCompletionSource<AsyncVoid>();

			task.ContinueWith(previousTask =>
			{
				if(previousTask.IsFaulted)
				{
					var aggregateException = previousTask.Exception;
					var innerException = aggregateException.Flatten().InnerExceptions.FirstOrDefault();
					onException(innerException ?? aggregateException);
					tcs.TrySetException(aggregateException);
				}
				else if(previousTask.IsCanceled)
				{
					if(onCancel != null)
						onCancel();
					tcs.TrySetCanceled();
				}
				else
				{
					try
					{
						onSuccess(previousTask);
						tcs.TrySetResult(default(AsyncVoid));
					}
					catch(Exception ex)
					{
						tcs.TrySetException(ex);
					}
				}
				if(finalAction != null) 
					finalAction();
			});

			return tcs.Task;
		}

 
	}
}