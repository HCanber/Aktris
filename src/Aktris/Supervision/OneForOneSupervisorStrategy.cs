using System;
using System.Collections.Generic;
using Aktris.Internals.Children;

namespace Aktris.Supervision
{
	public class OneForOneSupervisorStrategy : DeciderSupervisorStrategy
	{

		public OneForOneSupervisorStrategy(Func<Exception, SupervisorAction> decider = null, uint? maxNrOfRetries = null)
			: base(decider, maxNrOfRetries)
		{
		}


		protected override void HandleRestart(ActorRef failingActor, Exception cause, ChildRestartInfo restartInfo, IReadOnlyCollection<ChildRestartInfo> actorWithSiblings)
		{
			if(IsOkToRestart(restartInfo))
			{
				RestartActor(failingActor,cause,shouldSuspendFirst:false);
			}
		}

		protected override void HandleStop(ActorRef failingActor, Exception cause, ChildRestartInfo restartInfo, IReadOnlyCollection<ChildRestartInfo> actorWithSiblings)
		{
			StopActor(failingActor,cause);
		}
	}
}