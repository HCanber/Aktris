using System;
using System.Threading;
using System.Threading.Tasks;

namespace Aktris
{
	public class SelfScheduler : IScheduler
	{
		private readonly IScheduler _scheduler;
		private readonly ActorRef _self;

		public SelfScheduler(IScheduler scheduler, ActorRef self)
		{
			_scheduler = scheduler;
			_self = self;
		}

		public Task ScheduleSendSelf(int initialDelayMs, object message)
		{
			return _scheduler.ScheduleSendOnce(initialDelayMs, _self, _self, message);
		}

		public Task ScheduleSendSelf(int initialDelayMs, object message, CancellationToken cancellationToken)
		{
			return _scheduler.ScheduleSend(initialDelayMs, _self, _self, message, cancellationToken);
		}

		public Task ScheduleSendSelf(TimeSpan initialDelay, object message)
		{
			return _scheduler.ScheduleSendOnce(initialDelay, _self, _self, message);
		}

		public Task ScheduleSendSelf(TimeSpan initialDelay, object message, CancellationToken cancellationToken)
		{
			return _scheduler.ScheduleSendOnce(initialDelay, _self, _self, message, cancellationToken);
		}

		public Task ScheduleSend(int initialDelayMs, ActorRef receiver, ActorRef sender, object message, CancellationToken cancellationToken)
		{
			return _scheduler.ScheduleSend(initialDelayMs, receiver, sender, message, cancellationToken);
		}

		public Task ScheduleOnce(int initialDelayMs, Action action, CancellationToken cancellationToken)
		{
			return _scheduler.ScheduleOnce(initialDelayMs, action, cancellationToken);
		}



		public Task ScheduleSendSelfRepeatedly(int initialDelayMs, int intervalMs, object message)
		{
			return _scheduler.ScheduleSendRepeatedly(initialDelayMs, intervalMs, _self, _self, message);
		}

		public Task ScheduleSendSelfRepeatedly(int initialDelayMs, int intervalMs, object message, CancellationToken cancellationToken)
		{
			return _scheduler.ScheduleSendRepeatedly(initialDelayMs, intervalMs, _self, _self, message, cancellationToken);
		}

		public Task ScheduleSendSelfRepeatedly(TimeSpan initialDelay, TimeSpan interval, object message)
		{
			return _scheduler.ScheduleSendRepeatedly(initialDelay, interval, _self, _self, message);
		}

		public Task ScheduleSendSelfRepeatedly(TimeSpan initialDelay, TimeSpan interval, object message, CancellationToken cancellationToken)
		{
			return _scheduler.ScheduleSendRepeatedly(initialDelay, interval, _self, _self, message, cancellationToken);
		}

		public Task ScheduleSendRepeatedly(int initialDelayMs, int intervalMs, ActorRef receiver, ActorRef sender, object message, CancellationToken cancellationToken)
		{
			return _scheduler.ScheduleSendRepeatedly(initialDelayMs, intervalMs, receiver, sender, message, cancellationToken);
		}

		public Task Schedule(int initialDelayMs, int intervalMs, Func<bool> action, CancellationToken cancellationToken)
		{
			return _scheduler.Schedule(initialDelayMs, intervalMs, action, cancellationToken);
		}
	}
}