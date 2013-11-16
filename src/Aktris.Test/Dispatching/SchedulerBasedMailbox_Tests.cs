using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Aktris.Dispatching;
using Aktris.JetBrainsAnnotations;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Aktris.Test.Dispatching
{
// ReSharper disable InconsistentNaming
	public class SchedulerBasedMailbox_Tests
	{
		[Fact]
		public void When_enqueing_first_time_Then_an_action_is_scheduled()
		{
			var sender = A.Fake<ActorRef>();
			var scheduler = A.Fake<IScheduler>();

			var mailbox = new TestableMailbox(scheduler);
			mailbox.Enqueue(new Envelope("message",sender));

			A.CallTo(() => scheduler.Schedule(A<Action>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
		}
		[Fact]
		public void When_enqueing_second_without_mailbox_has_processed_first_message_Then_no_extra_action_is_scheduled()
		{
			var sender = A.Fake<ActorRef>();
			var scheduler = A.Fake<IScheduler>();

			var mailbox = new TestableMailbox(scheduler);
			mailbox.Enqueue(new Envelope("first message", sender));
			mailbox.Enqueue(new Envelope("second message", sender));
			A.CallTo(() => scheduler.Schedule(A<Action>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
		}

		[Fact]
		public void Given_two_messages_in_mailbox_When_scheduled_Then_both_messages_are_processed()
		{
			var sender = A.Fake<ActorRef>();
			var scheduler = new ManuallySyncronousScheduler();

			var mailbox = new TestableMailbox(scheduler);
			mailbox.Enqueue(new Envelope("first message", sender));
			mailbox.Enqueue(new Envelope("second message", sender));
			
			scheduler.ExecuteAll();
			var handledMessages = mailbox.HandledMessages.Select(e=>e.Message as string).ToList();
			handledMessages.Count.Should().Be(2);
			handledMessages[0].Should().Be("first message");
			handledMessages[1].Should().Be("second message");
		}

		private class TestableMailbox : SchedulerBasedMailbox
		{
			public ConcurrentQueue<Envelope> EnqueuedMessages = new ConcurrentQueue<Envelope>();
			public ConcurrentQueue<Envelope> HandledMessages = new ConcurrentQueue<Envelope>();
			public TestableMailbox([NotNull] IScheduler scheduler) : base(scheduler)
			{
			}

			protected override void InternalEnqueue(Envelope envelope)
			{
				EnqueuedMessages.Enqueue(envelope);
			}

			protected override bool HasMessagesEnqued()
			{
				return EnqueuedMessages.Count > 0;
			}

			protected override IEnumerable<Envelope> GetMessagesToProcess()
			{
				while(EnqueuedMessages.Count > 0)
				{
					Envelope message;
					if(EnqueuedMessages.TryDequeue(out message))
					{
						yield return message;
					}
				}
			}

			protected override void HandleMessage(Envelope envelope)
			{
				HandledMessages.Enqueue(envelope);
			}
		}

	}

	public class ManuallySyncronousScheduler : IScheduler
	{
		private ConcurrentQueue<Action> _queue=new ConcurrentQueue<Action>();
		public void Schedule(Action action)
		{
			_queue.Enqueue(action);
		}

		public void ExecuteAll()
		{
			while(_queue.Count > 0)
			{
				Action action;
				if(_queue.TryDequeue(out action)) action();
			}
		}
	}
	// ReSharper restore InconsistentNaming
}