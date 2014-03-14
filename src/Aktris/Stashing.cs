using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Aktris.Dispatching;
using Aktris.Exceptions;
using Aktris.Internals;

namespace Aktris
{
	/// <summary>
	/// Implements stashing functionality for an actor. An instance may not be shared between actors, 
	/// and it is not thread safe and may not be used concurrently.
	/// </summary>
	public class Stashing
	{
		private readonly InternalActorRef _actor;
		private readonly uint? _capacity;
		private ImmutableQueue<Envelope> _messages = ImmutableQueue<Envelope>.Empty;
		private int _count;
		private Envelope _lastMessage;
		public Stashing(InternalActorRef actor, uint? capacity)
		{
			_actor = actor;
			_capacity = capacity;
			var firstQueueable = actor.Mailbox as IFirstQueueable<Envelope>;
			if(firstQueueable == null)
			{
				throw new InvalidOperationException("The mailbox must implement " + typeof(IFirstQueueable<Envelope>));
			}
		}

		public void Stash(Envelope message)
		{
			if(!_messages.IsEmpty && ReferenceEquals(message, _lastMessage))
				throw new InvalidOperationException("Can not stash the same message more than once: " + message);
			if(_capacity.HasValue && _capacity.Value <= _count)
				throw new StashOverflowException("Reached the limit of " + _capacity.Value + ". Could not stash the message " + message);
			Enqueue(message);
		}

		private void Enqueue(Envelope message)
		{
			_messages = _messages.Enqueue(message);
			_lastMessage = message;
			_count++;
		}

		public void Unstash()
		{
			InternalUnstash(() => new[] { Dequeue() });
		}

		public void UnstashAll()
		{
			InternalUnstash(() =>
			{
				var allMessages = _messages.ToList();
				_messages = ImmutableQueue<Envelope>.Empty;
				_count = 0;
				return allMessages;
			});

		}


		private void InternalUnstash(Func<IReadOnlyList<Envelope>> getMessagesToUnstash)
		{
			//If there's nothing to unstash, just exit
			if(_messages.IsEmpty) return;

			//If the mailbox is DeadLetter, we just enqueues the messages to it. In this case they shouldn't end up first in the mailbox.
			var mailbox = _actor.Mailbox;
			var deadLetterMailbox = mailbox as DeadLetterMailbox;
			IReadOnlyList<Envelope> messagesToUnstash;
			if(deadLetterMailbox != null)
			{
				messagesToUnstash = getMessagesToUnstash();
				foreach(var envelope in messagesToUnstash)
				{
					deadLetterMailbox.Enqueue(envelope);
				}
				return;
			}

			//Fail if the mailbox is of invalid type
			var queueMailbox = mailbox as IFirstQueueable<Envelope>;
			if(queueMailbox == null)
				throw new InvalidOperationException("The mailbox must implement " + typeof(IFirstQueueable<Envelope>));

			//Enqueue the messages in the mailbox
			messagesToUnstash = getMessagesToUnstash();
			queueMailbox.EnqueueFirst(messagesToUnstash);
		}

		private Envelope Dequeue()
		{
			Envelope message;
			_messages.Dequeue(out message);
			_count--;
			return message;
		}
	}
}