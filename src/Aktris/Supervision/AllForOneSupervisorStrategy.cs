using System;
using System.Collections.Generic;
using System.Linq;
using Aktris.Internals.Children;
using Aktris.Internals.Helpers;

namespace Aktris.Supervision
{
	public class AllForOneSupervisorStrategy : DeciderSupervisorStrategy
	{
		private static readonly AllForOneSupervisorStrategy _DefaultInstance=new AllForOneSupervisorStrategy(SupervisorStrategy.DefaultDecider);

		public AllForOneSupervisorStrategy(Func<Exception, SupervisorAction> decider = null, uint? maxNrOfRetries = null, TimeSpan? withinTimeRange = null)
			: base(decider, maxNrOfRetries,withinTimeRange)
		{
		}

		public static AllForOneSupervisorStrategy DefaultAllForOne { get { return _DefaultInstance; } }


		protected override void HandleRestart(RestartableChildRestartInfo failed, Exception cause, IReadOnlyCollection<RestartableChildRestartInfo> allSiblingsIncludingFailed)
		{
			if(allSiblingsIncludingFailed.Count > 0)
			{
				var okToRestartAllChildren = allSiblingsIncludingFailed.All(IsOkToRestart);
				if(okToRestartAllChildren)
				{
					var failingActor = failed.Actor;
					allSiblingsIncludingFailed.ForEach(c => RestartActor(c.Actor, cause,shouldSuspendFirst: c.Actor!=failingActor));
				}
				else
				{
					allSiblingsIncludingFailed.ForEach(c => StopActor(c.Actor, cause));
				}
			}
		}

		protected override void HandleStop(RestartableChildRestartInfo failed, Exception cause, IReadOnlyCollection<RestartableChildRestartInfo> allSiblingsIncludingFailed)
		{
			allSiblingsIncludingFailed.ForEach(c => StopActor(c.Actor, cause));
		}

	}
}