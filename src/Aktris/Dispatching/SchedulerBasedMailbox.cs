using System;
using Aktris.JetBrainsAnnotations;

namespace Aktris.Dispatching
{
	public abstract class SchedulerBasedMailbox : MailboxBase
	{
		private readonly IScheduler _scheduler;

		protected SchedulerBasedMailbox([NotNull] IScheduler scheduler)
		{
			if(scheduler == null) throw new ArgumentNullException("scheduler");
			_scheduler = scheduler;
		}

		protected override void Schedule(Action action)
		{
			_scheduler.Schedule(action);
		}
	}
}