using System;
using Aktris.JetBrainsAnnotations;

namespace Aktris.Dispatching
{
	public abstract class SchedulerBasedMailbox : MailboxBase
	{
		private readonly IActionScheduler _actionScheduler;

		protected SchedulerBasedMailbox([NotNull] IActionScheduler scheduler)
		{
			if(scheduler == null) throw new ArgumentNullException("scheduler");
			_actionScheduler = scheduler;
		}

		protected override void Schedule(Action action)
		{
			_actionScheduler.Schedule(action);
		}
	}
}