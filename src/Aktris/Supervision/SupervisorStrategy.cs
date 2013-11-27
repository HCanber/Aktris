using System;
using System.Collections.Generic;
using Aktris.Internals.Children;

namespace Aktris.Supervision
{
	public abstract class SupervisorStrategy
	{
		private static readonly SupervisorStrategy _defaultStrategy = new OneForOneSupervisorStrategy(DefaultDecider);

		public bool HandleFailure(ActorRef child, Exception cause, ChildRestartInfo restartInfo, IReadOnlyCollection<ChildRestartInfo> children)
		{
			var action = DecideHowToHandle(cause) ?? SupervisorAction.Escalate;
			switch(action)
			{
				case SupervisorAction.Resume:
					ResumeChild(child, cause);
					break;
				case SupervisorAction.Restart:
					HandleRestart(child, cause, restartInfo, children);
					break;
				case SupervisorAction.Stop:
					HandleStop(child, cause, restartInfo, children);
					break;
				case SupervisorAction.Escalate:
					return false;
				default:
					throw new ArgumentOutOfRangeException("Unexpected " + typeof(SupervisorAction) + ": " + action);
			}
			return true;
		}

		protected abstract void HandleRestart(ActorRef child, Exception cause, ChildRestartInfo restartInfo, IReadOnlyCollection<ChildRestartInfo> children);
		protected abstract void HandleStop(ActorRef child, Exception cause, ChildRestartInfo restartInfo, IReadOnlyCollection<ChildRestartInfo> children);


		protected void ResumeChild(ActorRef child, Exception cause)
		{
		}

		protected void RestartChild(ActorRef child, Exception cause)
		{
		}
		protected void StopChild(ActorRef child, Exception cause)
		{
		}

		protected abstract SupervisorAction? DecideHowToHandle(Exception cause);

		public static SupervisorStrategy DefaultStrategy { get { return _defaultStrategy; } }

		public static SupervisorAction DefaultDecider(Exception exception)
		{
			//TODO:
			return SupervisorAction.Restart;
		}
	}
}