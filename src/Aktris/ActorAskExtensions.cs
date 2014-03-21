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
			return Ask<object>(actor, message, sender, (long)timeout.TotalMilliseconds);
		}

		public static AskResult<object> AskAndWait(this ActorRef actor, object message, ActorRef sender, long timeoutMilliseconds = Timeout.Infinite)
		{
			var task = Ask(actor, message, sender, timeoutMilliseconds);
			task.Wait();
			return task;
		}

		public static AskResult<object> Ask(this ActorRef actor, object message, ActorRef sender, long timeoutMilliseconds = Timeout.Infinite)
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
			return Ask<TResponse>(actor, message, sender, (long)timeout.TotalMilliseconds);
		}

		public static AskResult<TResponse> AskAndWait<TResponse>(this ActorRef actor, object message, ActorRef sender, long timeoutMilliseconds = Timeout.Infinite)
		{
			var result = Ask<TResponse>(actor, message, sender, timeoutMilliseconds);
			result.Wait();
			return result;
		}

		public static AskResult<TResponse> Ask<TResponse>(this ActorRef actor, object message, ActorRef sender, long timeoutMilliseconds = Timeout.Infinite)
		{
			var intActor = actor as InternalActorRef;
			if(intActor == null)
			{
				return new AskResult<TResponse>(Promise.Failed<TResponse>(new ArgumentException("Unsupported recipient ActorRef type, question not sent to " + actor, "actor")));
			}
			if(intActor.IsTerminated)
			{
				intActor.Send(message, sender);
				return new AskResult<TResponse>(Promise.Failed<TResponse>(new AskTimeoutException("Recipient " + actor + " had already been terminated.")));
			}
			if(timeoutMilliseconds < 0 && timeoutMilliseconds != Timeout.Infinite)
			{
				return new AskResult<TResponse>(Promise.Failed<TResponse>(new ArgumentOutOfRangeException("timeoutMilliseconds", timeoutMilliseconds, "Timeout must be positive or Infinite=" + Timeout.Infinite + ", question not sent to " + actor)));
			}
			var system = intActor.System;
			var promiseActor = PromiseActorRef.Create<TResponse>(system, timeoutMilliseconds, actor.ToString());
			actor.Send(message, promiseActor);	//Send to actor and expect response to promiseActor
			if(system.Settings.DebugMessages)
				system.EventStream.Publish(new DebugLogEvent(actor.Path.ToString(), intActor.SafeGetTypeForLogging(), "Ask: " + Envelope.ToString(sender,actor,message)));

			return new AskResult<TResponse>(promiseActor.Future);
		}
	}

	public class AskResult<T>
	{
		private readonly Task<T> _task;

		public AskResult(Task<T> task)
		{
			_task = task;
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

		public AskResult Result
		{
			get
			{
				Wait();
				if(_task.IsFaulted)
				{
					return HasTimedOut ? AskResult.TimedOut : AskResult.Failed;
				}
				if(!_task.IsCanceled) return AskResult.Succeeded;
				return AskResult.Unknown;
			}
		}

		public T Response { get { return Task.Result; } }

		public Exception FirstException { get { return _task.Exception == null ? null : _task.Exception.Flatten().InnerExceptions.FirstOrDefault(); } }
		public IReadOnlyList<Exception> AllExceptions { get { return _task.Exception == null ? null : _task.Exception.Flatten().InnerExceptions; } }

		public Task<T> Task { get { return _task; } }

		public void Handle(Action<T> onResponse = null, Action onTimeout = null, Action<IReadOnlyList<Exception>> onException = null)
		{
			_task.Wait();
			if(_task.IsCompleted)
			{
				if(onResponse!=null)
					onResponse(_task.Result);
			}
			else if(_task.IsFaulted)
			{
				var hasTimeoutHandler = onTimeout != null;
				var hasExceptionHandler = onException != null;
				var aggregateException = _task.Exception;
				if(hasTimeoutHandler || hasExceptionHandler)
				{
					var innerExceptions = aggregateException.Flatten().InnerExceptions;
					if(hasTimeoutHandler && innerExceptions.Any(e => e is AskTimeoutException || e is TimeoutException))
					{
						onTimeout();
						return;
					}
					if(hasExceptionHandler)
					{
						onException(innerExceptions);
						return;
					}
				}
				throw aggregateException;
			}
		}
		public TResult Handle<TResult>(Func<T,TResult> onResponse, Func<TResult> onTimeout, Func<IReadOnlyList<Exception>,TResult> onException)
		{
			_task.Wait();
			if(_task.IsCompleted)
			{
				return onResponse(_task.Result);
			}

			var aggregateException = _task.Exception;
			var innerExceptions = aggregateException == null
				? (IReadOnlyList<Exception>) EmptyReadonlyCollection<Exception>.Instance
				: (IReadOnlyList<Exception>) aggregateException.Flatten().InnerExceptions;
			if(_task.IsCanceled || innerExceptions.Any(e => e is AskTimeoutException || e is TimeoutException))
			{
				return onTimeout();
			}
			return onException(innerExceptions);
		}

		public void ThrowIfFailed()
		{
			Task.Wait();
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