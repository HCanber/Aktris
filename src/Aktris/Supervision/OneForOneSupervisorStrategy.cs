using System;
using System.Collections.Generic;
using System.Threading;
using Aktris.Internals.Children;

namespace Aktris.Supervision
{
	public class OneForOneSupervisorStrategy : DeciderSupervisorStrategy
	{
		private static readonly OneForOneSupervisorStrategy _DefaultInstance = new OneForOneSupervisorStrategy(decider: SupervisorStrategy.DefaultDecider);

		public OneForOneSupervisorStrategy(Func<Exception, SupervisorAction> decider = null, uint? maxNrOfRetries = null, TimeSpan? withinTimeRange=null)
			: base(decider, maxNrOfRetries, withinTimeRange)
		{
		}

		public static OneForOneSupervisorStrategy DefaultOneForOne { get { return _DefaultInstance; } }


		protected override void HandleRestart(RestartableChildRestartInfo failed, Exception cause, IReadOnlyCollection<RestartableChildRestartInfo> allSiblingsIncludingFailed)
		{
			if(IsOkToRestart(failed))
				RestartActor(failed.Actor, cause, shouldSuspendFirst: false);
			else
				StopActor(failed.Actor, cause);

		}

		protected override void HandleStop(RestartableChildRestartInfo failed, Exception cause, IReadOnlyCollection<RestartableChildRestartInfo> allSiblingsIncludingFailed)
		{
			StopActor(failed.Actor, cause);
		}
	}
}