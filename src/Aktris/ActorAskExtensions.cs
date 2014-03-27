using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Aktris.Dispatching;
using Aktris.Exceptions;
using Aktris.Internals;
using Aktris.Internals.Concurrency;
using Aktris.Internals.Helpers;
using Aktris.Internals.Logging;
using Aktris.JetBrainsAnnotations;
using Aktris.Messages;

namespace Aktris
{
	public static class ActorAskExtensions
	{
		public static AskResult<object> AskAndWait(this ActorRef actor, object message, ActorRef sender, TimeSpan timeout)
		{
			var task = Ask(actor, message, sender, timeout);
			task.Wait();
			return task;
		}

		public static AskResult<object> Ask(this ActorRef actor, object message, ActorRef sender, TimeSpan timeout)
		{
			return Ask<object>(actor, message, sender, (int)timeout.TotalMilliseconds);
		}

		public static AskResult<object> AskAndWait(this ActorRef actor, object message, ActorRef sender, int timeoutMilliseconds = Timeout.Infinite)
		{
			var task = Ask(actor, message, sender, timeoutMilliseconds);
			task.Wait();
			return task;
		}

		public static AskResult<object> Ask(this ActorRef actor, object message, ActorRef sender, int timeoutMilliseconds = Timeout.Infinite)
		{
			return Ask<object>(actor, message, sender, timeoutMilliseconds);
		}

		public static AskResult<TResponse> AskAndWait<TResponse>(this ActorRef actor, object message, ActorRef sender, TimeSpan timeout)
		{
			var task = Ask<TResponse>(actor, message, sender, timeout);
			task.Wait();
			return task;
		}

		public static AskResult<TResponse> Ask<TResponse>(this ActorRef actor, object message, ActorRef sender, TimeSpan timeout)
		{
			return Ask<TResponse>(actor, message, sender, (int)timeout.TotalMilliseconds);
		}

		public static AskResult<TResponse> AskAndWait<TResponse>(this ActorRef actor, object message, ActorRef sender, int timeoutMilliseconds = Timeout.Infinite)
		{
			var result = Ask<TResponse>(actor, message, sender, timeoutMilliseconds);
			result.Wait();
			return result;
		}

		public static AskResult<TResponse> Ask<TResponse>(this ActorRef actor, object message, ActorRef sender, int timeoutMilliseconds = Timeout.Infinite)
		{
			var intActor = actor as InternalActorRef;
			if(intActor == null)
			{
				return new AskResult<TResponse>(Promise.Failed<TResponse>(new ArgumentException("Unsupported recipient ActorRef type, question not sent to " + actor, "actor")), actor);
			}
			if(intActor.IsTerminated)
			{
				intActor.Send(message, sender);
				return new AskResult<TResponse>(Promise.Failed<TResponse>(new AskTimeoutException("Recipient " + actor + " had already been terminated.")), actor);
			}
			if(timeoutMilliseconds < 0 && timeoutMilliseconds != Timeout.Infinite)
			{
				return new AskResult<TResponse>(Promise.Failed<TResponse>(new ArgumentOutOfRangeException("timeoutMilliseconds", timeoutMilliseconds, "Timeout must be positive or Infinite=" + Timeout.Infinite + ", question not sent to " + actor)), actor);
			}
			var system = intActor.System;
			var promiseActor = PromiseActorRef.Create<TResponse>(system, timeoutMilliseconds, actor.ToString());
			actor.Send(message, promiseActor);	//Send to actor and expect response to promiseActor
			if(system.Settings.DebugMessages)
				system.EventStream.Publish(new DebugLogEvent(actor.Path.ToString(), intActor.SafeGetTypeForLogging(), "Ask: " + Envelope.ToString(sender, actor, message)));

			return new AskResult<TResponse>(promiseActor.Future, actor);
		}
	}

	public class AskResult<T>
	{
		private readonly Task<T> _task;
		private readonly ActorRef _askedActor;

		public AskResult(Task<T> task, ActorRef askedActor)
		{
			_task = task;
			_askedActor = askedActor;
		}
		private AskResult<TResult> CreateNew<TResult>(Task<TResult> task)
		{
			return new AskResult<TResult>(task, _askedActor);
		}

		public bool HasTimedOut
		{
			get { return _task.ContainsException<AskTimeoutException, TimeoutException>(); }
		}

		public void Wait()
		{
			if(_task.IsCompleted) return;
			try
			{
				_task.Wait();
			}
			catch(AggregateException e)
			{
				//Swallow exceptions
			}
		}



		public T Response { get { return _task.Result; } }

		public Exception FirstException { get { return _task.Exception == null ? null : _task.Exception.Flatten().InnerExceptions.FirstOrDefault(); } }
		public IReadOnlyList<Exception> AllExceptions { get { return _task.Exception == null ? null : _task.Exception.Flatten().InnerExceptions; } }

		//public Task<T> Task { get { return _task; } }


		public void PipeTo(ActorRef actor, Func<T, object> onResult = null, Func<IReadOnlyList<Exception>, object> onExceptionOrTimeout = null)
		{
			PipeTo(actor, onResult, onExceptionOrTimeout == null ? (Func<object>)null : () => onExceptionOrTimeout(null), onExceptionOrTimeout);
		}

		public void PipeTo(ActorRef actor, Func<T, object> onResult = null, Func<object> onTimeout = null, Func<IReadOnlyList<Exception>, object> onException = null)
		{

			InternalHandle(
				onSuccess: response => actor.Send(onResult == null ? response : onResult(response), _askedActor),
				onException: aggregateException =>
				{
					if(aggregateException == null)
						actor.Send(onTimeout == null ? new AskTimeout() : onTimeout(), _askedActor);
					else
					{
						var exceptions = aggregateException.Flatten().InnerExceptions;
						if(exceptions.Count == 1)
						{
							var exception = exceptions[0];
							if(exception is TimeoutException)
								actor.Send(onTimeout == null ? new AskTimeout() : onTimeout(), _askedActor);
							else
								actor.Send(onException == null ? new Status.Failure(exception) : onException(exceptions), _askedActor);
						}
						else
							actor.Send(onException == null ? new Status.Failure(aggregateException) : onException(exceptions), _askedActor);
					}
				}
				);
		}

		public AskResult<T> Handle(Action<T> onResponse, Action<IReadOnlyList<Exception>> onTimeoutOrOnException)
		{
			var responseHandler = onResponse != null ? r => { onResponse(r); return r; } : (Func<T, T>)null;
			var timeoutHandler = onTimeoutOrOnException != null ? () => onTimeoutOrOnException(null) : (Action)null;
			return Handle<T>(responseHandler, timeoutHandler, onTimeoutOrOnException);
		}

		public AskResult<T> Handle(Action<T> onResponse = null, Action onTimeout = null, Action<IReadOnlyList<Exception>> onException = null)
		{
			var responseHandler = onResponse != null ? r => { onResponse(r); return r; } : (Func<T, T>)null;
			return Handle<T>(responseHandler, onTimeout, onException);
		}

		public AskResult<TResult> Handle<TResult>(Func<T, TResult> onResponse = null, Action onTimeout = null, Action<IReadOnlyList<Exception>> onException = null)
		{
			var tcs = new TaskCompletionSource<TResult>();
			_task.ContinueWith(t =>
			{
				try
				{
					if(t.IsCanceled)
						tcs.TrySetCanceled();
					else if(t.IsFaulted)
					{
						var hasTimeoutHandler = onTimeout != null;
						var hasExceptionHandler = onException != null;
						var aggregateException = t.Exception;
						var innerExceptions = aggregateException.Flatten().InnerExceptions;
						var isTimeout = innerExceptions.Any(e => e is AskTimeoutException || e is TimeoutException);
						if(isTimeout)
						{
							if(hasTimeoutHandler)
								onTimeout();
						}
						else if(hasExceptionHandler)
						{
							onException(innerExceptions);
						}
						tcs.TrySetException(aggregateException);
					}
					else
					{
						if(onResponse != null)
						{
							var result = onResponse(t.Result);
							tcs.SetResult(result);
						}
					}
				}
				catch(Exception e)
				{
					tcs.TrySetException(e);
				}
			});

			return CreateNew(tcs.Task);
		}

		public AskResult<TResult> Transform<TResult>(Func<T, TResult> transformResponse)
		{
			var newTask = _task.Then(t=>transformResponse(t.Result));
			return CreateNew(newTask);
		}

		public AskResult<TResult> Transform<TResult>([NotNull] Func<T, TResult> onResponse, [NotNull] Func<TResult> onTimeout, [NotNull] Func<IReadOnlyList<Exception>, TResult> onException)
		{
			if(onResponse == null) throw new ArgumentNullException("onResponse");
			if(onTimeout == null) throw new ArgumentNullException("onTimeout");
			if(onException == null) throw new ArgumentNullException("onException");

			var tcs = new TaskCompletionSource<TResult>();
			_task.ContinueWith(t =>
			{
				try
				{
					if(t.IsCanceled)
						tcs.TrySetCanceled();
					else if(t.IsFaulted)
					{
						var aggregateException = t.Exception;
						var innerExceptions = aggregateException.Flatten().InnerExceptions;
						var isTimeout = innerExceptions.Any(e => e is AskTimeoutException || e is TimeoutException);
						if(isTimeout)
						{
							var result = onTimeout();
							tcs.SetResult(result);
						}
						else
						{
							var result = onException(innerExceptions);
							tcs.SetResult(result);
						}
					}
					else
					{
						var result = onResponse(t.Result);
						tcs.SetResult(result);
					}
				}
				catch(Exception e)
				{
					tcs.TrySetException(e);
				}
			});

			return CreateNew(tcs.Task);
		}

		private void InternalHandle([NotNull] Action<T> onSuccess, [NotNull] Action<AggregateException> onException)
		{
			if(onSuccess == null) throw new ArgumentNullException("onSuccess");
			if(onException == null) throw new ArgumentNullException("onException");

			_task.ContinueWith(t =>
			{
				if(t.IsCanceled)
				{
					onException(null);
				}
				else
				{
					if(t.IsFaulted)
						onException(t.Exception);
					else
						onSuccess(t.Result);
				}
			});
		}


		public void ThrowIfFailed()
		{
			_task.Wait();
			if(_task.IsFaulted)
			{
				throw FirstException;
			}
		}

	}


	public enum AskResult
	{
		Succeeded,
		TimedOut,
		Failed,
		Unknown,
	}
}