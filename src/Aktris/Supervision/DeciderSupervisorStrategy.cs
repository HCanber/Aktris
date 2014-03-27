using System;
using System.Runtime.Remoting.Messaging;
using Aktris.Internals.Children;

namespace Aktris.Supervision
{
	public abstract class DeciderSupervisorStrategy : SupervisorStrategy
	{
		private readonly int _maxNrOfRetries;
		private readonly Func<Exception, SupervisorAction> _decider;
		private int _withinTimeRangeMs;

		protected DeciderSupervisorStrategy(Func<Exception, SupervisorAction> decider, uint? maxNrOfRetries, TimeSpan? withinTimeRange)
		{
			_maxNrOfRetries = maxNrOfRetries.HasValue ? (int)maxNrOfRetries : -1;
			_decider = decider ?? DefaultDecider;
			_withinTimeRangeMs = withinTimeRange.HasValue ? (int)withinTimeRange.Value.TotalMilliseconds : -1;
		}

		protected Func<Exception, SupervisorAction> Decider { get { return _decider; } }

		protected override SupervisorAction? DecideHowToHandle(Exception cause)
		{
			return _decider(cause);
		}

		protected bool IsOkToRestart(RestartableChildRestartInfo restartableChildRestartInfo)
		{
			return restartableChildRestartInfo.RequestRestartPermission(_maxNrOfRetries, _withinTimeRangeMs);
		}
	}
}