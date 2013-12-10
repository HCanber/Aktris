using System;
using System.Collections.Generic;
using Aktris.Internals;
using Aktris.Internals.Children;

namespace Aktris.Supervision
{
	public abstract class SupervisorStrategy
	{
		private static readonly SupervisorStrategy _DefaultStrategy = new OneForOneSupervisorStrategy(DefaultDecider);

		public bool HandleFailure(ActorRef failingActor, Exception cause, ChildRestartInfo restartInfo, IReadOnlyCollection<ChildRestartInfo> actorWithSiblings)
		{
			var action = DecideHowToHandle(cause) ?? SupervisorAction.Escalate;
			switch(action)
			{
				case SupervisorAction.Resume:
					ResumeActor(failingActor, cause);
					break;
				case SupervisorAction.Restart:
					HandleRestart(failingActor, cause, restartInfo, actorWithSiblings);
					break;
				case SupervisorAction.Stop:
					HandleStop(failingActor, cause, restartInfo, actorWithSiblings);
					break;
				case SupervisorAction.Escalate:
					return false;
				default:
					throw new ArgumentOutOfRangeException("Unexpected " + typeof(SupervisorAction) + ": " + action);
			}
			return true;
		}

		protected abstract void HandleRestart(ActorRef failingActor, Exception cause, ChildRestartInfo restartInfo, IReadOnlyCollection<ChildRestartInfo> actorWithSiblings);
		protected abstract void HandleStop(ActorRef failingActor, Exception cause, ChildRestartInfo restartInfo, IReadOnlyCollection<ChildRestartInfo> actorWithSiblings);


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
			//TODO:
			return SupervisorAction.Restart;
		}
	}
}