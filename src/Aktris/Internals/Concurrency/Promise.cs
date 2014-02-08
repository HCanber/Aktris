using System;
using System.Threading.Tasks;

namespace Aktris.Internals.Concurrency
{
	public class Promise<T> : IDisposable
	{
		private bool _isDisposed; //Automatically initialized to false;
		private readonly TaskCompletionSource<T> _taskCompletionSource;

		public Promise()
		{
			_taskCompletionSource = new TaskCompletionSource<T>();
		}

		public Task<T> Future { get { return _taskCompletionSource.Task; } }

		public bool IsCompleted { get { return _taskCompletionSource.Task.IsCompleted; } }

		public void Success(T result)
		{
			_taskCompletionSource.SetResult(result);
		}

		public bool TrySuccess(T result)
		{
			return _taskCompletionSource.TrySetResult(result);
		}

		public void Failure(Exception exception)
		{
			_taskCompletionSource.SetException(exception);
		}

		public bool TryFailure(Exception exception)
		{
			return _taskCompletionSource.TrySetException(exception);
		}


		public void Dispose()
		{
			Dispose(true);
			//Take this object off the finalization queue and prevent finalization code for this object
			//from executing a second time.
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			// If disposing equals false, the method has been called by the
			// runtime from inside the finalizer and you should not reference
			// other objects. Only unmanaged resources can be disposed.

			try
			{
				//Make sure Dispose does not get called more than once, by checking the disposed field
				if(!_isDisposed)
				{
					if(disposing)
					{
						_taskCompletionSource.TrySetCanceled();
					}
					//Clean up unmanaged resources
				}
				_isDisposed = true;
			}
			finally
			{
				// base.dispose(disposing);
			}
		}

	}
}