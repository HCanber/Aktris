using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Aktris.Internals;
using Aktris.Internals.SystemMessages;

namespace Aktris.Dispatching
{
	public abstract class MailboxBase : Mailbox
	{
		private volatile int _mailboxStatus = MailboxStatus.Open;
		private ILocalActorRef _actor;
		private readonly ConcurrentQueue<SystemMessageEnvelope> _systemMessagesQueue = new ConcurrentQueue<SystemMessageEnvelope>();


		public void SetActor(ILocalActorRef actor)
		{
			if(_actor != null) throw new InvalidOperationException(StringFormat.SafeFormat("Trying to reuse a Mailbox. It's already in use for {0} and cannot be used for {1}", _actor, actor));
			_actor = actor;
			Register(actor);
		}

		protected virtual void Register(ILocalActorRef actor)
		{
		}

		public void Enqueue(Envelope envelope)
		{
			InternalEnqueue(envelope);

			ScheduleIfNeeded();
		}

		public void EnqueueSystemMessage(SystemMessageEnvelope envelope)
		{
			_systemMessagesQueue.Enqueue(envelope);
			ScheduleIfNeeded();

		}

		private void ProcessMessages()
		{
			try
			{
				ProcessSystemMessages();
				ProcessNormalMessages();
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

		private void ProcessSystemMessages()
		{
			SystemMessageEnvelope envelope;
			while(_systemMessagesQueue.TryDequeue(out envelope))
			{
				_actor.HandleSystemMessage(envelope);
			}
		}

		private void ProcessNormalMessages()
		{
			var messagesToProcess = GetMessagesToProcess();
			foreach(var message in messagesToProcess)
			{
				HandleMessage(message);
			}
		}

		protected abstract bool HasMessagesEnqued();

		protected abstract IEnumerable<Envelope> GetMessagesToProcess();

		protected virtual void HandleMessage(Envelope envelope)
		{
			_actor.HandleMessage(envelope);
		}

		protected abstract void Schedule(Action action);

		protected abstract void InternalEnqueue(Envelope envelope);



		private void ScheduleIfNeeded()
		{
			if(_actor != null && UpdateStatusIf(MailboxStatus.IsNotClosedOrScheduled, MailboxStatus.SetScheduled))
			{
				Schedule(ProcessMessages);
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