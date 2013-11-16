using System;
using System.Collections.Generic;
using System.Threading;
using Aktris.Internals;

namespace Aktris.Dispatching
{
	public abstract class MailboxBase : Mailbox
	{
		private long _numberOfAttachedActors = 0;
		private volatile int _mailboxStatus = MailboxStatus.Open;

		public long NumberOfAttachedActors { get { return _numberOfAttachedActors; } }

		public void Attach(ILocalActorRef actor)
		{
			Register(actor);
		}

		protected virtual void Register(ILocalActorRef actor)
		{
			Interlocked.Increment(ref _numberOfAttachedActors);
		}

		public void Enqueue(Envelope envelope)
		{
			InternalEnqueue(envelope);

			ScheduleIfNeeded();
		}


		private void Run()
		{
			try
			{
				var messagesToProcess = GetMessagesToProcess();
				foreach(var message in messagesToProcess)
				{
					HandleMessage(message);
				}
			}
			finally
			{
				SetIdle();
				if(HasMessagesEnqued())
				{
					ScheduleIfNeeded();
				}
			}
		}

		protected abstract bool HasMessagesEnqued();

		protected abstract IEnumerable<Envelope> GetMessagesToProcess();

		protected virtual void HandleMessage(Envelope envelope)
		{
			throw new NotImplementedException();
		}

		protected abstract void Schedule(Action action);

		protected abstract void InternalEnqueue(Envelope envelope);



		private void ScheduleIfNeeded()
		{
			if(UpdateStatusIf(MailboxStatus.IsNotClosedOrScheduled, MailboxStatus.SetScheduled))
			{
				Schedule(Run);
			}
		}

		private void SetIdle()
		{
			UpdateStatus(MailboxStatus.SetIdle);
		}

		private void UpdateStatus(Func<int, int> statusUpdater)
		{
			// ReSharper disable once CSharpWarnings::CS0420    Ok to ignore "a reference to a volatile field will not be treated as volatile" for interlocked calls http://msdn.microsoft.com/en-us/library/4bw5ewxy(VS.80).aspx
			InterlockedSpin.Swap(ref _mailboxStatus, statusUpdater);
		}

		private bool UpdateStatusIf(Predicate<int> predicate, Func<int, int> statusUpdater)
		{
			// ReSharper disable once CSharpWarnings::CS0420    Ok to ignore "a reference to a volatile field will not be treated as volatile" for interlocked calls http://msdn.microsoft.com/en-us/library/4bw5ewxy(VS.80).aspx
			return InterlockedSpin.BreakableSwap(ref _mailboxStatus, status =>
			{
				var canUpdate = predicate(status);
				return Tuple.Create(canUpdate, statusUpdater(status), !canUpdate);
			});
		}

		private static class MailboxStatus
		{
			public const int Open = 0;
			public const int Closed = 1;
			public const int Scheduled = 2;
			private const int ScheduledOrClosedMask = 3;

			public static bool IsNotClosedOrScheduled(int status)
			{
				return (status & MailboxStatus.ScheduledOrClosedMask) != 0;
			}
			public static int SetScheduled(int status)
			{
				return status | MailboxStatus.Scheduled;
			}
			public static int SetIdle(int status)
			{
				return status & ~MailboxStatus.Scheduled;
			}
		}
	}
}