using System;
using System.Threading;
using System.Threading.Tasks;
using Aktris.Internals.Concurrency;

namespace Aktris.Internals
{
	public class TaskBasedScheduler : IScheduler
	{
		public Task ScheduleSendOnce(int initialDelayMs, ActorRef receiver, ActorRef sender, object message, CancellationToken cancellationToken)
		{
			var internalReceiver = (InternalActorRef)receiver;
			return InternalScheduleOnce(initialDelayMs, () =>
			{
				if(!internalReceiver.IsTerminated)
					receiver.Send(message, sender);
			}, cancellationToken);
		}

		public Task ScheduleOnce(int initialDelayMs, Action action, CancellationToken cancellationToken)
		{
			return InternalScheduleOnce(initialDelayMs, action, cancellationToken);
		}

		private Task InternalScheduleOnce(int initialDelayMs, Action action, CancellationToken token)
		{
			if(initialDelayMs == Timeout.Infinite) return TaskExt.CreateInfiniteTask();
			if(initialDelayMs <= 0)
			{
				action();
				return TaskExt.CreateCompletedTask();
			}

			return Task.Delay(initialDelayMs, token).Then(t=>action());
		}

		public Task ScheduleSendRepeatedly(int initialDelayMs, int intervalMs, ActorRef receiver, ActorRef sender, object message, CancellationToken cancellationToken)
		{
			var internalReceiver = (InternalActorRef)receiver;
			return InternalScheduleRepeatedly(initialDelayMs, intervalMs, () =>
			{
				if(!internalReceiver.IsTerminated) return false;
				receiver.Send(message, sender);
				return true;
			}, cancellationToken);
		}

		public Task Schedule(int initialDelayMs, int intervalMs, Func<bool> action, CancellationToken cancellationToken)
		{
			return InternalScheduleRepeatedly(initialDelayMs, intervalMs, action, cancellationToken);
		}


		private Task InternalScheduleRepeatedly(int initialDelayMs, int intervalMs, Func<bool> action, CancellationToken token)
		{
			if(initialDelayMs == Timeout.Infinite) return TaskExt.CreateInfiniteTask();
			return InternalScheduleRepeatedlyAsync(initialDelayMs, intervalMs, action,token);
		}

		private async Task InternalScheduleRepeatedlyAsync(int initialDelayMs, int intervalMs, Func<bool> action, CancellationToken token)
		{
			await Task.Delay(initialDelayMs, token);
			if(intervalMs>=0)
			{
				while(!token.IsCancellationRequested)
				{
					if(!action()) return;
					await Task.Delay(intervalMs, token);
				}
			}
		}
	}
}