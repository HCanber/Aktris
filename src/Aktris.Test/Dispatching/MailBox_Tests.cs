using System;
using System.Collections.Generic;
using System.Linq;
using Aktris.Dispatching;
using Aktris.Internals;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Aktris.Test.Dispatching
{
	// ReSharper disable once InconsistentNaming
	public class Mailbox_Tests
	{
		[Fact]
		public void Given_a_Mailbox_with_no_actor_When_calling_ScheduleIfNeeded_Then_it_should_not_schedule()
		{
			var mailbox = new TestMailbox();
			mailbox.CallScheduleIfNeeded();
			mailbox.NumberOfScheduleCalls.Should().Be(0);
		}

		[Fact]
		public void Given_a_Mailbox_When_setting_a_actor_Then_a_Schedule_is_performed()
		{
			var mailbox = new TestMailbox();
			mailbox.SetActor(A.Dummy<InternalActorRef>());
			mailbox.NumberOfScheduleCalls.Should().Be(1);
		}

		[Fact]
		public void Given_a_Mailbox_that_has_not_yet_been_scheduled_When_calling_ScheduleIfNeeded_Then_a_new_Schedule_is_not_performed()
		{
			var mailbox = new TestMailbox();
			mailbox.SetActor(A.Dummy<InternalActorRef>());
			var scheduleCallsBefore = mailbox.NumberOfScheduleCalls;
			mailbox.CallScheduleIfNeeded();
			mailbox.NumberOfScheduleCalls.Should().Be(scheduleCallsBefore);
		}

		[Fact]
		public void Given_a_Mailbox_When_calling_Suspend_Then_it_returns_true()
		{
			var mailbox = CreateMailboxWithActor();
			mailbox.CallSuspend().Should().BeTrue();
		}

		[Fact]
		public void Given_a_Mailbox_with_a_message_When_processing_Scheduled_calls_Then_it_becomes_idle()
		{
			var mailbox = CreateMailboxWithActor();
			mailbox.Enqueue(CreateDummyMessage());
			mailbox.ProcessScheduledCalls();
			mailbox.GetStatus().IsIdle().Should().BeTrue();
		}

		[Fact]
		public void Given_suspended_Mailbox_When_calling_Suspend_Then_it_returns_false()
		{
			var mailbox = CreateMailboxWithActor();
			mailbox.CallSuspend();
			mailbox.CallSuspend().Should().BeFalse();
		}

		[Fact]
		public void Given_twice_suspended_Mailbox_When_calling_Resume_twice_Then_it_returns_true_the_second_time_only()
		{
			var mailbox = CreateMailboxWithActor();
			mailbox.CallSuspend();
			mailbox.CallSuspend();

			mailbox.CallResume().Should().BeFalse();
			mailbox.CallResume().Should().BeTrue();
		}

		[Fact]
		public void Given_a_Mailbox_When_calling_Resume_many_times_Then_it_returns_true()
		{
			var mailbox = CreateMailboxWithActor();

			//Resuming should always return true, when it is not suspended, even if it already was running.
			mailbox.CallResume().Should().BeTrue();
			mailbox.CallResume().Should().BeTrue();
			mailbox.CallResume().Should().BeTrue();
		}

		[Fact]
		public void Given_suspended_Mailbox_When_enquing_message_Then_it_is_enqued_but_not_scheduled()
		{
			var mailbox = CreateMailboxWithActor();
			mailbox.CallSuspend();
			mailbox.Enqueue(CreateDummyMessage());
			mailbox.EnqueuedMessages.Should().HaveCount(1);
			mailbox.NumberOfScheduleCalls.Should().Be(0);
		}

		[Fact]
		public void Given_suspended_Mailbox_with_messages_enqued_When_resuming_Then_it_is_scheduled()
		{
			var mailbox = CreateMailboxWithActor();
			mailbox.CallSuspend();
			mailbox.Enqueue(CreateDummyMessage());
			mailbox.Resume();
			mailbox.NumberOfScheduleCalls.Should().Be(1);
		}

		[Fact]
		public void Given_closed_Mailbox_When_enqueing_message_Then_it_is_not_scheduled()
		{
			var mailbox = CreateMailboxWithActor();
			mailbox.CallClose();
			mailbox.Enqueue(CreateDummyMessage());
			mailbox.NumberOfScheduleCalls.Should().Be(0);
		}

		[Fact]
		public void Given_closed_Mailbox_When_calling_Suspend_Then_it_returns_false_and_remains_closed()
		{
			var mailbox = CreateMailboxWithActor();
			mailbox.CallClose();
			mailbox.CallSuspend().Should().BeFalse();
			mailbox.GetStatus().IsClosed().Should().BeTrue();
		}
		[Fact]
		public void Given_closed_Mailbox_When_calling_Resume_Then_it_returns_false_and_remains_closed()
		{
			var mailbox = CreateMailboxWithActor();
			mailbox.CallClose();
			mailbox.CallResume().Should().BeFalse();
			mailbox.GetStatus().IsClosed().Should().BeTrue();
		}

		[Fact]
		public void When_enqueing_messages_during_handling_message_Then_it_is_scheduled()
		{
			var actor = A.Fake<InternalActorRef>();
			var mailbox = CreateMailboxWithActor(actor);
			A.CallTo(() => actor.HandleMessage(A<Envelope>.Ignored)).Invokes(() => mailbox.Enqueue(CreateDummyMessage()));
			mailbox.Enqueue(CreateDummyMessage());
			mailbox.ProcessScheduledCalls();
			mailbox.NumberOfScheduleCalls.Should().Be(1);
		}

		private static Envelope CreateDummyMessage()
		{
			return new Envelope(A.Dummy<ActorRef>(), "A Dummy Message", A.Dummy<ActorRef>());
		}

		private static TestMailbox CreateMailboxWithActor()
		{
			return CreateMailboxWithActor(A.Dummy<InternalActorRef>());
		}

		private static TestMailbox CreateMailboxWithActor(InternalActorRef internalActorRef)
		{
			var mailbox = new TestMailbox();
			mailbox.SetActor(internalActorRef);
			mailbox.ProcessScheduledCalls();
			mailbox.Reset();
			return mailbox;
		}

		private class TestMailbox : MailboxBase
		{
			public int NumberOfScheduleCalls { get { return Scheduled.Count; } }
			public List<Envelope> EnqueuedMessages = new List<Envelope>();
			public List<Action> Scheduled = new List<Action>();

			protected override bool HasMessagesEnqued()
			{
				return EnqueuedMessages.Count > 0;
			}

			protected override IEnumerable<Envelope> GetMessagesToProcess()
			{
				var messagesToProcess = EnqueuedMessages;
				EnqueuedMessages = new List<Envelope>();
				return messagesToProcess;
			}

			protected override void Schedule(Action action)
			{
				Scheduled.Add(action);
			}

			protected override void InternalEnqueue(Envelope envelope)
			{
				EnqueuedMessages.Add(envelope);
			}

			public void Reset()
			{
				ResetScheduledCalls();
				EnqueuedMessages=new List<Envelope>();
				
			}

			public void ResetScheduledCalls()
			{
				Scheduled=new List<Action>();
			}

			public void ProcessScheduledCalls()
			{
				var scheduled = Scheduled;
				Scheduled=new List<Action>();
				foreach(var action in scheduled)
				{
					action();
				}
			}

			public void CallScheduleIfNeeded() { ScheduleIfNeeded(); }

			public bool CallSuspend() { return Suspend(); }
			public bool CallResume() { return Resume(); }
			public bool CallClose() { return Close(); }
			public int GetStatus(){return Status;}
		}
	}
}