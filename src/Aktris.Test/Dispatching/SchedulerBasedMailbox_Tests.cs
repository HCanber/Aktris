using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Aktris.Dispatching;
using Aktris.Internals;
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
		public void Given_a_mailbox_with_no_actor_attached_When_enqueing_Then_no_action_is_scheduled()
		{
			var receiver = A.Fake<ActorRef>();
			var sender = A.Fake<ActorRef>();
			var scheduler = A.Fake<IScheduler>();

			var mailbox = new TestableMailbox(scheduler);
			mailbox.Enqueue(new Envelope(receiver, "message", sender));

			A.CallTo(() => scheduler.Schedule(A<Action>.Ignored)).MustNotHaveHappened();
		}
		[Fact]
		public void When_enqueing_first_time_Then_an_action_is_scheduled()
		{
			var receiver = A.Fake<ActorRef>();
			var sender = A.Fake<ActorRef>();
			var scheduler = A.Fake<IScheduler>();

			var mailbox = new TestableMailbox(scheduler);
			mailbox.Attach(A.Fake<ILocalActorRef>());
			mailbox.Enqueue(new Envelope(receiver, "message", sender));

			A.CallTo(() => scheduler.Schedule(A<Action>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
		}
		[Fact]
		public void When_enqueing_second_without_mailbox_has_processed_first_message_Then_no_extra_action_is_scheduled()
		{
			var receiver = A.Fake<ActorRef>();
			var sender = A.Fake<ActorRef>();
			var scheduler = A.Fake<IScheduler>();

			var mailbox = new TestableMailbox(scheduler);
			mailbox.Attach(A.Fake<ILocalActorRef>());
			mailbox.Enqueue(new Envelope(receiver, "first message", sender));
			mailbox.Enqueue(new Envelope(receiver, "second message", sender));
			A.CallTo(() => scheduler.Schedule(A<Action>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
		}

		[Fact]
		public void Given_two_messages_in_mailbox_When_scheduled_Then_both_messages_are_processed()
		{
			var receiver = A.Fake<ActorRef>();
			var sender = A.Fake<ActorRef>();
			var scheduler = new ManuallySyncronousScheduler();

			var mailbox = new TestableMailbox(scheduler);
			mailbox.Attach(A.Fake<ILocalActorRef>());
			mailbox.Enqueue(new Envelope(receiver, "first message", sender));
			mailbox.Enqueue(new Envelope(receiver, "second message", sender));
			
			scheduler.ExecuteAll();
			var handledMessages = mailbox.HandledMessages.Select(e=>e.Message as string).ToList();
			handledMessages.Count.Should().Be(2);
			handledMessages[0].Should().Be("first message");
			handledMessages[1].Should().Be("second message");
		}

		[Fact]
		public void Given_mailbox_with_messages_When_scheduler_processes_Then_mailbox_calls_actor_with_messages()
		{
			var receiver = A.Fake<ActorRef>();
			var sender = A.Fake<ActorRef>();
			var scheduler = new ManuallySyncronousScheduler();

			var mailbox = new TestableMailbox(scheduler);
			var fakeActor = A.Fake<ILocalActorRef>();
			var actorMessages = new List<Envelope>();
			A.CallTo(() => fakeActor.HandleMessage(A<Envelope>.Ignored)).Invokes(x => actorMessages.Add(x.GetArgument<Envelope>(0)));
			mailbox.Attach(fakeActor);
			mailbox.Enqueue(new Envelope(receiver, "first message", sender));
			mailbox.Enqueue(new Envelope(receiver, "second message", sender));

			scheduler.ExecuteAll();
			actorMessages.Count.Should().Be(2);
			actorMessages[0].Message.Should().Be("first message");
			actorMessages[1].Message.Should().Be("second message");
		}
		private class TestableMailbox : SchedulerBasedMailbox
		{
			public ILocalActorRef Actor { get; set; }
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
				base.HandleMessage(envelope);
			}

			protected override void Register(ILocalActorRef actor)
			{
				Actor = actor;
			}

			protected override IEnumerable<ILocalActorRef> GetRecipients(Envelope envelope)
			{
				yield return Actor;
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