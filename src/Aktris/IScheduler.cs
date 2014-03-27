using System;
using System.Threading;
using System.Threading.Tasks;

namespace Aktris
{
	public interface IScheduler
	{
		Task ScheduleSend(int initialDelayMs, ActorRef receiver, ActorRef sender, object message, CancellationToken cancellationToken);
		Task ScheduleOnce(int initialDelayMs, Action action, CancellationToken cancellationToken);

		Task ScheduleSendRepeatedly(int initialDelayMs, int intervalMs, ActorRef receiver, ActorRef sender, object message, CancellationToken cancellationToken);
		Task Schedule(int initialDelayMs, int intervalMs, Func<bool> action, CancellationToken cancellationToken);
	}

	public static class SchedulerExtensions
	{
		public static Task ScheduleSendOnce(this IScheduler scheduler, TimeSpan initialDelay, ActorRef receiver, ActorRef sender, object message)
		{
			return scheduler.ScheduleSend((int) initialDelay.TotalMilliseconds, receiver, sender, message, CancellationToken.None);
		}

		public static Task ScheduleSendOnce(this IScheduler scheduler, TimeSpan initialDelay, ActorRef receiver, ActorRef sender, object message, CancellationToken cancellationToken)
		{
			return scheduler.ScheduleSend((int) initialDelay.TotalMilliseconds, receiver, sender, message, cancellationToken);
		}

		public static Task ScheduleSendOnce(this IScheduler scheduler, int initialDelayMs, ActorRef receiver, ActorRef sender, object message)
		{
			return scheduler.ScheduleSend(initialDelayMs, receiver, sender, message, CancellationToken.None);
		}


		public static Task ScheduleOnce(this IScheduler scheduler, TimeSpan initialDelay, Action action)
		{
			return scheduler.ScheduleOnce((int)initialDelay.TotalMilliseconds, action, CancellationToken.None);
		}

		public static Task ScheduleOnce(this IScheduler scheduler, TimeSpan initialDelay, Action action, CancellationToken cancellationToken)
		{
			return scheduler.ScheduleOnce((int)initialDelay.TotalMilliseconds, action, cancellationToken);
		}

		public static Task ScheduleOnce(this IScheduler scheduler, int initialDelayMs, Action action)
		{
			return scheduler.ScheduleOnce(initialDelayMs, action, CancellationToken.None);
		}




		public static Task ScheduleSendRepeatedly(this IScheduler scheduler, TimeSpan initialDelay, TimeSpan interval, ActorRef receiver, ActorRef sender, object message)
		{
			return scheduler.ScheduleSendRepeatedly((int)initialDelay.TotalMilliseconds, (int)interval.TotalMilliseconds, receiver, sender, message, CancellationToken.None);
		}

		public static Task ScheduleSendRepeatedly(this IScheduler scheduler, TimeSpan initialDelay, TimeSpan interval, ActorRef receiver, ActorRef sender, object message, CancellationToken cancellationToken)
		{
			return scheduler.ScheduleSendRepeatedly((int)initialDelay.TotalMilliseconds, (int)interval.TotalMilliseconds,receiver,sender,message, cancellationToken);
		}

		public static Task ScheduleSendRepeatedly(this IScheduler scheduler, int initialDelayMs, int intervalMs, ActorRef receiver, ActorRef sender, object message)
		{
			return scheduler.ScheduleSendRepeatedly(initialDelayMs, intervalMs, receiver, sender, message, CancellationToken.None);
		}


		public static Task Schedule(this IScheduler scheduler, TimeSpan initialDelay, TimeSpan interval, Func<bool> action, CancellationToken cancellationToken)
		{
			return scheduler.Schedule((int)initialDelay.TotalMilliseconds, (int)interval.TotalMilliseconds, action, cancellationToken);
		}

		public static Task Schedule(this IScheduler scheduler, TimeSpan initialDelay, TimeSpan interval, Func<bool> action)
		{
			return scheduler.Schedule((int)initialDelay.TotalMilliseconds, (int)interval.TotalMilliseconds, action, CancellationToken.None);
		}

		public static Task Schedule(this IScheduler scheduler, int initialDelayMs, int intervalMs, Func<bool> action)
		{
			return scheduler.Schedule(initialDelayMs, intervalMs, action, CancellationToken.None);
		}
	}
}
