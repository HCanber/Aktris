using System;
using System.Threading;
using System.Threading.Tasks;
using Aktris.Exceptions;
using Aktris.Internals;
using Aktris.Internals.Concurrency;

namespace Aktris
{
	public static class ActorAskExtensions
	{
		public static Task<object> Ask(this ActorRef actor, object message, ActorRef sender)
		{
			return Ask(actor, message, sender, Timeout.Infinite);
		}

		public static Task<object> Ask(this ActorRef actor, object message, ActorRef sender, TimeSpan timeout)
		{
			return Ask(actor, message, sender, (long)timeout.TotalMilliseconds);
		}

		public static Task<object> Ask(this ActorRef actor, object message, ActorRef sender, long timeoutMilliseconds)
		{
			var intActor = actor as InternalActorRef;
			if(intActor == null)
			{
				return Promise.Failed<object>(new ArgumentException("Unsupported recipient ActorRef type, question not sent to " + actor,"actor"));
			}
			if(intActor.IsTerminated)
			{
				intActor.Send(message, sender);
				return Promise.Failed<object>(new AskTimeoutException("Recipient " + actor + " had already been terminated."));
			}
			if(timeoutMilliseconds < 0 && timeoutMilliseconds != Timeout.Infinite)
			{
				return Promise.Failed<object>(new ArgumentOutOfRangeException("timeoutMilliseconds", timeoutMilliseconds, "Timeout must be positive or Infinite=" + Timeout.Infinite + ", question not sent to " + actor));
			}
			var promiseActor = PromiseActorRef.Create(intActor.System, timeoutMilliseconds, actor.ToString());
			actor.Send(message, promiseActor);	//Send to actor and expect response to promiseActor
			return promiseActor.Future;
		}
	}
}