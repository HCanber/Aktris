using System;
using Aktris.Internals.Children;

namespace Aktris.Supervision
{
	public abstract class DeciderSupervisorStrategy : SupervisorStrategy
	{
		private readonly uint? _maxNrOfRetries;
		private readonly Func<Exception, SupervisorAction> _decider;

		protected DeciderSupervisorStrategy(Func<Exception, SupervisorAction> decider, uint? maxNrOfRetries)
		{
			_maxNrOfRetries = maxNrOfRetries;
			_decider = decider ?? DefaultDecider;
		}

		protected uint? MaxNrOfRetries { get { return _maxNrOfRetries; } }

		protected Func<Exception, SupervisorAction> Decider { get { return _decider; } }

		protected override SupervisorAction? DecideHowToHandle(Exception cause)
		{
			return _decider(cause);
		}

		protected bool IsOkToRestart(ChildRestartInfo restartInfo)
		{
			if(!_maxNrOfRetries.HasValue) return true;
			return restartInfo.NumberOfRestarts < _maxNrOfRetries.Value;
		}
	}
}