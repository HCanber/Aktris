using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Aktris.Internals;
using Aktris.Internals.Logging;
using Aktris.Internals.SystemMessages;
using Aktris.JetBrainsAnnotations;

namespace Aktris.Dispatching
{
	public abstract class MailboxBase : Mailbox
	{
		[DebuggerDisplay("{GetMailboxStatusForDebug(),nq}")]
		private volatile int _mailboxStatus = MailboxStatus.Open;
		private InternalActorRef _actor;
		private readonly ConcurrentQueue<SystemMessageEnvelope> _systemMessagesQueue = new ConcurrentQueue<SystemMessageEnvelope>();

		[DebuggerDisplay("{GetMailboxStatusForDebug(),nq}")]
		protected internal int Status { get { return _mailboxStatus; } }

		public bool IsSuspended { get { return _mailboxStatus.IsSuspended(); } }
		public bool IsClosed { get { return Status.IsClosed(); } }

		public void SetActor(InternalActorRef actor)
		{
			if(_actor != null) throw new InvalidOperationException(StringFormat.SafeFormat("Trying to reuse a Mailbox. It's already in use for {0} and cannot be used for {1}", _actor, actor));
			_actor = actor;
			Register(actor);
			ScheduleIfNeeded();
		}

		public void DetachActor(InternalActorRef actor)
		{
			if(_actor == actor) _actor = null;
		}

		protected virtual void Register(InternalActorRef actor)
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
			ScheduleIfNeeded(hasSystemMessage: true);
		}

		private void ProcessMessages()
		{
			try
			{
				ProcessAllSystemMessages();
				ProcessNormalMessages();
			}
			catch(Exception ex)
			{
				//TODO: Log
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

		private void ProcessAllSystemMessages()
		{
			SystemMessageEnvelope envelope;
			while(_systemMessagesQueue.TryDequeue(out envelope))
			{
				var system = _actor.System;
				if(system.Settings.DebugSystemMessages && !_actor.IsLogger)
				{
					system.EventStream.Publish(new DebugLogEvent(_actor.Path.ToString(),_actor.SafeGetTypeForLogging(),"Processing system message: " + envelope));
				}
				_actor.HandleSystemMessage(envelope);
			}
		}

		private void ProcessNormalMessages()
		{
			if(!Status.IsSuspendedOrClosed())
			{
				Envelope message;
				while(TryGetMessageToProcess(out message))
				{
					var system = _actor.System;
					if(system.Settings.DebugMessages && !_actor.IsLogger)
					{
						system.EventStream.Publish(new DebugLogEvent(_actor.Path.ToString(), _actor.SafeGetTypeForLogging(), "Processing message: " + message));
					}
					HandleMessage(message);
					ProcessAllSystemMessages();
				}
			}
		}

		protected abstract bool HasMessagesEnqued();

		protected abstract bool TryGetMessageToProcess(out Envelope message);

		protected abstract IEnumerable<Envelope> GetMessagesToProcess();

		protected virtual void HandleMessage(Envelope envelope)
		{
			_actor.HandleMessage(envelope);
		}

		protected abstract void Schedule(Action action);

		protected abstract void InternalEnqueue(Envelope envelope);


		protected void ScheduleIfNeeded(bool hasSystemMessage = false)
		{
			if(_actor != null)
			{
				if(UpdateStatusIf(status => hasSystemMessage ? status.IsOpenAndIdle() : status.IsOpenAndIdleAndNotSuspended(), MailboxStatus.SetScheduled))
				{
					Schedule(ProcessMessages);
				}
			}
		}

		public void Suspend(InternalActorRef actorRef)
		{
			if(Equals(actorRef.Mailbox, this))
			{
				Suspend();
			}
		}

		/// <summary>Suspends this instance and returns<c>True</c> if it was running.</summary>
		protected internal bool Suspend()
		{
			var status = Status;
			if(status.IsClosed())
			{
				UpdateStatus(MailboxStatus.SetClosed);
				return false;
			}
			UpdateStatus(MailboxStatus.IncreaseSuspendCount);
			return status.GetSuspendCount() == 0;
		}

		public void Resume(InternalActorRef actorRef)
		{
			if(Equals(actorRef.Mailbox, this))
			{
				Resume();
			}
		}

		/// <summary>Resumes this instance and returns <c>True</c> if it became (or already was) running.</summary>
		protected internal bool Resume()
		{
			var status = Status;
			bool becameResumed;
			if(status.IsClosed())
			{
				UpdateStatus(MailboxStatus.SetClosed);
				becameResumed = false;
			}
			else
			{
				var newStatus = UpdateStatus(MailboxStatus.DecreaseSuspendedCount);
				becameResumed = !newStatus.IsSuspended();
			}
			if(becameResumed)
			{
				ScheduleIfNeeded();
			}
			return becameResumed;
		}

		/// <summary>Closes this instance and returns<c>True</c> if it was running (i.e. not closed already).</summary>
		protected internal bool Close()
		{
			while(true)
			{
				var status = Status;
				var wasAlreadyClosed = status.IsClosed();
				UpdateStatus(MailboxStatus.SetClosed);
				return !wasAlreadyClosed;
			}
		}

		private void SetIdle()
		{
			UpdateStatus(MailboxStatus.SetIdle);
		}

		private int UpdateStatus(Func<int, int> statusUpdater)
		{
			// ReSharper disable once CSharpWarnings::CS0420    Ok to ignore "a reference to a volatile field will not be treated as volatile" for interlocked calls http://msdn.microsoft.com/en-us/library/4bw5ewxy(VS.80).aspx
			return InterlockedSpin.Swap(ref _mailboxStatus, statusUpdater);
		}

		/// <summary>
		/// Updates the status if the predicate is satisfied. Returns true if the value was updated, i.e. if the predicate returned true.
		/// </summary>
		private bool UpdateStatusIf(Predicate<int> predicate, Func<int, int> statusUpdater)
		{
			// ReSharper disable once CSharpWarnings::CS0420    Ok to ignore "a reference to a volatile field will not be treated as volatile" for interlocked calls http://msdn.microsoft.com/en-us/library/4bw5ewxy(VS.80).aspx
			return InterlockedSpin.ConditionallySwap(ref _mailboxStatus, status =>
			{
				var canUpdate = predicate(status);
				return Tuple.Create(canUpdate, canUpdate ? statusUpdater(status) : 0);
			});
		}

		internal string GetMailboxStatusForDebug()
		{
			return _mailboxStatus.ToDebugString();
		}
	}



	public static class MailboxStatus
	{
		//Bits:
		// ...76543210
		//          ||
		//          |0=Open, 1=Closed
		//          0=Idle, 1=Scheduled
		// ---------
		// if any is 1 then it's Suspended. Suspended count by shifting >>2
		public const int Open = 0;
		private const int _Closed = 1;
		private const int _Scheduled = 2;
		private const int _ScheduledOrClosedMask = 3;
		private const int _SuspendMask = ~3;
		private const int _SuspendUnit = 4;
		private const int _SuspendedOrClosedMask = ~2;

		public static bool IsClosedOrScheduled(this int status)
		{
			return (status & _ScheduledOrClosedMask) != 0;
		}

		public static bool IsOpenAndIdle(this int status)
		{
			return (status & _ScheduledOrClosedMask) == 0;
		}

		public static bool IsOpenAndIdleAndNotSuspended(this int status)
		{
			return status == 0;
		}



		public static bool IsIdle(this int status)
		{
			return (status & _Scheduled) == 0;
		}

		public static bool IsScheduled(this int status)
		{
			return (status & _Scheduled) != 0;
		}

		public static int SetScheduled(this int status)
		{
			return status | _Scheduled;
		}

		public static int SetIdle(this int status)
		{
			return status & ~_Scheduled;
		}

		public static int SetClosed(this int status)
		{
			return _Closed;
		}

		public static bool IsClosed(this int status)
		{
			return status == _Closed;
		}

		public static bool IsSuspended(this int status)
		{
			return (status & _SuspendMask) != 0;
		}

		public static int IncreaseSuspendCount(this int status)
		{
			return status + _SuspendUnit;
		}

		public static int DecreaseSuspendedCount(this int status)
		{
			return IsSuspended(status) ? status - _SuspendUnit : status;
		}

		public static int GetSuspendCount(this int status)
		{
			return status >> 2;
		}

		public static bool IsSuspendedOrClosed(this int status)
		{
			return (status & _SuspendedOrClosedMask) != 0;
		}

		public static string ToDebugString(this int status)
		{
			var s = new List<string>();
			s.Add(IsClosed(status) ? "Closed" : "Open");
			s.Add(IsScheduled(status) ? "Scheduled" : "Idle");
			if(IsSuspended(status)) s.Add("Suspended (" + GetSuspendCount(status) + " times)");
			return string.Join(", ", s) + " [" + status + "]";
		}

	}
}