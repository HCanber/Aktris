using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Aktris.Exceptions;
using Aktris.Internals.Concurrency;
using Aktris.Internals.Helpers;
using Aktris.Internals.SystemMessages;
using Aktris.JetBrainsAnnotations;
using Aktris.Messages;

namespace Aktris.Internals
{
	public class PromiseActorRef : EmptyLocalActorRef
	{
		private readonly IPromise<object> _promise;
		private readonly ActorRef _deadLetters;
		private const int _StartedState = 0;
		private const int _StoppedState = 1;
		private volatile int _state = _StartedState;
		private static readonly IImmutableSet<InternalActorRef> _EmptyActorRefSet = ImmutableHashSet<InternalActorRef>.Empty;
		private IImmutableSet<InternalActorRef> _watchedBy = _EmptyActorRefSet;

		public PromiseActorRef([NotNull] ActorPath path, IPromise<object> promise, ActorRef deadLetters)
			: base(path)
		{
			_promise = promise;
			_deadLetters = deadLetters;
		}

		public Task<object> Future { get { return _promise.Future; } }

		public override void Send(object message, ActorRef sender)
		{
			if(_state == _StoppedState)
			{
				_deadLetters.Send(message, sender);
			}
			else
			{
				if(message == null) throw new ArgumentNullException("message");
				var promiseWasCompleted	=
					(Match<Status.Failure>(message, failure => _promise.TryFailure(failure.Exception))
					?? Match<Status.Success>(message, success => _promise.TrySuccess(success.Status))
					?? MatchAll(message, other => _promise.TrySuccess(other))
					).GetValueOrDefault();		//The MatchAll will always return a bool, so the bool? always has a value

				if(!promiseWasCompleted) _deadLetters.Send(message, sender);
			}
		}

		public override void SendSystemMessage(SystemMessage message, ActorRef sender)
		{
			var ignored = Match<TerminateActor>(message, _ => Stop())
										|| Match<ActorTerminated>(message, m => Send(new WatchedActorTerminated(m.TerminatedActor), this))
										|| Match<WatchActor>(message, m =>
										{
											if(m.Watchee == this && m.Watcher != this)
												if(!AddWatcher(m.Watcher))
													m.Watcher.SendSystemMessage(new ActorTerminated(this), this);
										})
										|| Match<UnwatchActor>(message, m =>
										{
											if(m.Watchee == this && m.Watcher != this) RemoveWatcher(m.Watcher);
										});  //Ignore other messages. Just drop them
		}

		public override bool IsTerminated { get { return _state == _StoppedState; } }


		public override void Stop()
		{
			var wasStartedNowChangedToStopped = Interlocked.CompareExchange(ref _state, _StoppedState, _StartedState) == _StartedState;
			if(wasStartedNowChangedToStopped)
			{
				_promise.TryFailure(new ActorKilledException(this, "Stopped"));
				var watchers = ClearWatchers();
				watchers.ForEach(watcher => watcher.SendSystemMessage(new ActorTerminated(this), this));
			}
		}

		private IReadOnlyCollection<InternalActorRef> ClearWatchers()
		{
			var set = Interlocked.Exchange(ref _watchedBy, _EmptyActorRefSet);
			return set;
		}

		private bool AddWatcher(InternalActorRef watcher)
		{
			var wasAddedToSet = InterlockedSpin.ConditionallySwap(ref _watchedBy, existing =>
			{
				var newSet = existing.Add(watcher);
				var wasAdded = !ReferenceEquals(existing, newSet);
				const bool shouldUpdate = true;
				return Tuple.Create(shouldUpdate,newSet, wasAdded);
			});
			return wasAddedToSet;
		}

		private void RemoveWatcher(InternalActorRef watcher)
		{
			InterlockedSpin.Swap(ref _watchedBy, existing => existing.Remove(watcher));
		}

		private static bool Match<T>(object message, Action<T> handler) where T : class
		{
			return PatternMatcher.Match<T>(message, handler);
		}

		private static bool? Match<T>(object message, Func<T, bool> handler) where T : class
		{
			return PatternMatcher<bool?>.Match<T>(message,m=> handler(m));
		}

		private static bool? MatchAll(object message, Func<object,bool> handler)
		{
			return handler(message);
		}


		public static PromiseActorRef Create(ActorSystem system, long timeoutMilliseconds, string targetName)
		{
			//Create a Promise and a PromiseActorRef
			var internalSystem = (InternalActorSystem)system;
			var tempPath = internalSystem.CreateTempActorPath();
			var promise = new Promise<object>();
			var promiseActorRef = new PromiseActorRef(tempPath, promise, system.DeadLetters);

			var timeoutableTask = promise.Future.TimeoutAfter(timeoutMilliseconds);

			timeoutableTask
				.Finally(ex =>
				{
					if(ex is TimeoutException)
						promise.Failure(new AskTimeoutException(string.Format("Ask timed out on {0} after {1}ms", targetName, timeoutMilliseconds)));
					else
						promise.Failure(ex);
				},
				() => promiseActorRef.Stop());

			return promiseActorRef;
		}

		public override ActorRef CreateActor(ActorCreationProperties actorCreationProperties, string name = null)
		{
			throw new System.NotImplementedException();
		}
	}
}