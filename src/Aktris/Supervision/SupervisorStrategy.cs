using System;
using System.Collections.Generic;
using Aktris.Exceptions;
using Aktris.Internals;
using Aktris.Internals.Children;

namespace Aktris.Supervision
{
	public abstract class SupervisorStrategy
	{
		private static readonly SupervisorStrategy _DefaultStrategy = new OneForOneSupervisorStrategy(decider: DefaultDecider);

		public bool HandleFailure(RestartableChildRestartInfo failedActorRestartInfo, Exception cause, IReadOnlyCollection<RestartableChildRestartInfo> allSiblingsIncludingFailingActor)
		{
			var action = DecideHowToHandle(cause) ?? SupervisorAction.Escalate;
			var failingActor = failedActorRestartInfo.Actor;
			switch(action)
			{
				case SupervisorAction.Resume:
					ResumeActor(failingActor, cause);
					break;
				case SupervisorAction.Restart:
					HandleRestart(failedActorRestartInfo, cause, allSiblingsIncludingFailingActor);
					break;
				case SupervisorAction.Stop:
					HandleStop(failedActorRestartInfo, cause, allSiblingsIncludingFailingActor);
					break;
				case SupervisorAction.Escalate:
					return false;
				default:
					throw new ArgumentOutOfRangeException("Unexpected " + typeof(SupervisorAction) + ": " + action);
			}
			return true;
		}

		protected abstract void HandleRestart(RestartableChildRestartInfo failed, Exception cause, IReadOnlyCollection<RestartableChildRestartInfo> allSiblingsIncludingFailed);
		protected abstract void HandleStop(RestartableChildRestartInfo failed, Exception cause, IReadOnlyCollection<RestartableChildRestartInfo> allSiblingsIncludingFailed);


		protected void ResumeActor(ActorRef actor, Exception cause)
		{
			var internalActorRef = ((InternalActorRef)actor);
			internalActorRef.Resume(cause);
		}

		protected void RestartActor(ActorRef actor, Exception cause, bool shouldSuspendFirst)
		{
			var internalActorRef = ((InternalActorRef)actor);
			if(shouldSuspendFirst)
			{
				internalActorRef.Suspend();
			}
			internalActorRef.Restart(cause);

		}

		protected void StopActor(ActorRef actor, Exception cause)
		{
			((InternalActorRef) actor).Stop();
		}

		protected abstract SupervisorAction? DecideHowToHandle(Exception cause);

		public static SupervisorStrategy DefaultStrategy { get { return _DefaultStrategy; } }

		public static SupervisorAction DefaultDecider(Exception exception)
		{
			if(exception is CreateActorFailedException || exception is ActorKilledException)				//TODO: || exception is DeathPactException
				return SupervisorAction.Stop;
			return SupervisorAction.Restart;
		}
	}
}