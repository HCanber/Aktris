using System;
using System.Collections.Generic;
using Aktris.Internals.Children;

namespace Aktris.Supervision
{
	public class OneForOneSupervisorStrategy : SupervisorStrategyBase
	{

		public OneForOneSupervisorStrategy(Func<Exception, SupervisorAction> decider = null, uint? maxNrOfRetries = null)
			: base(decider, maxNrOfRetries)
		{
		}


		protected override void HandleRestart(ActorRef child, Exception cause, ChildRestartInfo restartInfo, IReadOnlyCollection<ChildRestartInfo> children)
		{
			if(IsOkToRestart(restartInfo))
			{
				RestartChild(child,cause);
			}
		}

		protected override void HandleStop(ActorRef child, Exception cause, ChildRestartInfo restartInfo, IReadOnlyCollection<ChildRestartInfo> children)
		{
			StopChild(child,cause);
		}
	}
}