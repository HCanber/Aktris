using System;
using System.Threading.Tasks;
using Aktris.Exceptions;
using Aktris.Internals;
using Aktris.Internals.Concurrency;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Aktris.Test
{
	public class Actor_Ask_Tests
	{
		[Fact]
		public void When_asking_a_non_InternalActorRef_Then_a_failed_task_is_returned()
		{
			var task = A.Fake<ActorRef>().Ask("the message",null);
			task.IsFaulted.Should().BeTrue();
			task.Exception.ContainsException<ArgumentException>().Should().BeTrue();
		}

		[Fact]
		public void When_asking_with_negative_timeout_Then_a_failed_task_is_returned()
		{
			var task = A.Fake<InternalActorRef>().Ask("the message", null,-4711);
			task.IsFaulted.Should().BeTrue();
			task.Exception.ContainsException<ArgumentOutOfRangeException>().Should().BeTrue();
		}

		[Fact]
		public void Given_a_terminated_actor_When_asking_Then_a_failed_task_is_returned()
		{
			var actorRef = A.Fake<InternalActorRef>();
			A.CallTo(() => actorRef.IsTerminated).Returns(true);
			var task = actorRef.Ask("the message", null);
			task.IsFaulted.Should().BeTrue();
			task.Exception.ContainsException<AskTimeoutException>().Should().BeTrue();
		}

		[Fact]
		public void When_timing_out_Then_the_task_is_running_but_eventually_fails()
		{
			var actorRef = A.Fake<InternalActorRef>();
			A.CallTo(() => actorRef.System).Returns(new TestActorSystem());

			var task = actorRef.Ask("the message", null,100);
			var taskStatus = task.Status;
			(taskStatus != TaskStatus.Faulted && taskStatus != TaskStatus.RanToCompletion && taskStatus != TaskStatus.Canceled).Should().BeTrue();
			Assert.Throws<AggregateException>(() => task.Wait()).ContainsException<AskTimeoutException>().Should().BeTrue();
		}

		[Fact]
		public void When_asking_Then_response_is_returned_in_task()
		{
			var system = new TestActorSystem();
			system.Start();
			var actor = system.CreateActor<EchoActor>().InnerActorRef;
			var response = actor.Ask("message",null);
			response.Result.Should().Be(":message:");
		}

		private class EchoActor : Actor
		{
			public EchoActor()
			{
				Receive<string>(msg=>Sender.Reply(":"+msg+":"));
			}
		}
	}
}