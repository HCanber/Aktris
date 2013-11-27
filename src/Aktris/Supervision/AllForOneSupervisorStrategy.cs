using System;
using System.Collections.Generic;
using System.Linq;
using Aktris.Internals.Children;
using Aktris.Internals.Helpers;

namespace Aktris.Supervision
{
	public class AllForOneSupervisorStrategy : SupervisorStrategyBase
	{

		public AllForOneSupervisorStrategy(Func<Exception, SupervisorAction> decider = null, uint? maxNrOfRetries = null)
			: base(decider, maxNrOfRetries)
		{
		}


		protected override void HandleRestart(ActorRef child, Exception cause, ChildRestartInfo restartInfo, IReadOnlyCollection<ChildRestartInfo> children)
		{
			if(children.Count > 0)
			{
				var okToRestartAllChildren = children.All(IsOkToRestart);
				if(okToRestartAllChildren)
				{
					children.ForEach(c => RestartChild(c.Child, cause));
				}
				else
				{
					children.ForEach(c => StopChild(c.Child, cause));
				}
			}
		}

		protected override void HandleStop(ActorRef child, Exception cause, ChildRestartInfo restartInfo, IReadOnlyCollection<ChildRestartInfo> children)
		{
			children.ForEach(c => StopChild(c.Child, cause));
		}

	}
}