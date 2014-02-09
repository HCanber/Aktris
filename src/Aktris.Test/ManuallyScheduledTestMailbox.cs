using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Aktris.Dispatching;
using Aktris.Internals;
using Aktris.Internals.SystemMessages;

namespace Aktris.Test
{
	public class DelayedTestMailbox : Mailbox
	{
		private readonly Mailbox _mailbox;
		private bool _isOpen;
		private ConcurrentQueue<object> _enqueuedMessages = new ConcurrentQueue<object>();
		private readonly object _lock = new object();

		public DelayedTestMailbox(Mailbox mailbox)
		{
			_mailbox = mailbox;
		}

		public void Open()
		{
			ConcurrentQueue<object> enqueuedMessages;
			lock(_lock)
			{
				_isOpen = true;
				enqueuedMessages = _enqueuedMessages;
				_enqueuedMessages = new ConcurrentQueue<object>();

				foreach(var m in enqueuedMessages)
				{
					var envelope = m as Envelope;
					if(envelope != null)
						_mailbox.Enqueue(envelope);
					else
						_mailbox.EnqueueSystemMessage((SystemMessageEnvelope)m);
				}
			}
		}

		public void Close()
		{
			lock(_lock)
			{
				_isOpen = false;
			}
		}

		public void SetActor(InternalActorRef actor)
		{
			_mailbox.SetActor(actor);
		}

		public void Enqueue(Envelope envelope)
		{
			lock(_lock)
			{
				if(_isOpen)
					_mailbox.Enqueue(envelope);
				else
					_enqueuedMessages.Enqueue(envelope);
			}
		}

		public void EnqueueSystemMessage(SystemMessageEnvelope envelope)
		{
			lock(_lock)
			{
				if(_isOpen)
					_mailbox.EnqueueSystemMessage(envelope);
				else
					_enqueuedMessages.Enqueue(envelope);
			}
		}

		public void Suspend(InternalActorRef actor)
		{
			_mailbox.Suspend(actor);
		}

		public void Resume(InternalActorRef actor)
		{
			_mailbox.Resume(actor);
		}

		public bool IsSuspended
		{
			get { return _mailbox.IsSuspended; }
		}

		public bool IsClosed
		{
			get { return _mailbox.IsClosed; }
		}
		
		public void DetachActor(InternalActorRef actor)
		{
			_mailbox.DetachActor(actor);
		}
	}
}