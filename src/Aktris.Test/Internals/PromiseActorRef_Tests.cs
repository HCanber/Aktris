using System;
using Aktris.Exceptions;
using Aktris.Internals;
using Aktris.Internals.Concurrency;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Aktris.Test.Internals
{
// ReSharper disable once InconsistentNaming
	public class PromiseActorRef_Tests
	{
		[Fact]
		public void Given_a_non_stopped_Then_it_is_not_terminated()
		{
			var actorRef = new PromiseActorRef(A.Fake<ActorPath>(), A.Fake<IPromise<object>>(), A.Fake<ActorRef>());
			actorRef.IsTerminated.Should().BeFalse();
		}

		[Fact]
		public void Given_a_stopped_Then_it_is_terminated()
		{
			var actorRef = new PromiseActorRef(A.Fake<ActorPath>(), A.Fake<IPromise<object>>(), A.Fake<ActorRef>());
			actorRef.Stop();
			actorRef.IsTerminated.Should().BeTrue();
		}

		[Fact]
		public void Given_a_stopped_When_sending_message_Then_it_is_forwarded_to_DeadLetter()
		{
			var deadLetters = A.Fake<ActorRef>();
			var actorRef = new PromiseActorRef(A.Fake<ActorPath>(), A.Fake<IPromise<object>>(), deadLetters);
			actorRef.Stop();

			var sender = A.Fake<ActorRef>();
			actorRef.Send("test", sender);
			A.CallTo(() => deadLetters.Send("test", sender)).MustHaveHappened();
		}

		[Fact]
		public void Given_a_non_stopped_When_sending_Success_Then_promise_success_is_called()
		{
			var promise = A.Fake<IPromise<object>>();
			var actorRef = new PromiseActorRef(A.Fake<ActorPath>(), promise, A.Fake<ActorRef>());

			var sender = A.Fake<ActorRef>();
			actorRef.Send(new Status.Success("test"), sender);

			A.CallTo(() => promise.TrySuccess("test")).MustHaveHappened();
		}

		[Fact]
		public void Given_a_completed_promise_When_sending_Then_message_is_forwarded_to_deadletters()
		{
			var promise = A.Fake<IPromise<object>>();
			var deadLetters = A.Fake<ActorRef>();
			var actorRef = new PromiseActorRef(A.Fake<ActorPath>(), promise, deadLetters);
			A.CallTo(() => promise.TrySuccess("test")).Returns(false);
			var sender = A.Fake<ActorRef>();
			actorRef.Send("test", sender);

			A.CallTo(() => deadLetters.Send("test", sender)).MustHaveHappened();

		}


		[Fact]
		public void Given_a_non_stopped_When_sending_Failure_Then_promise_failure_is_called()
		{
			var promise = A.Fake<IPromise<object>>();
			var actorRef = new PromiseActorRef(A.Fake<ActorPath>(), promise, A.Fake<ActorRef>());

			var sender = A.Fake<ActorRef>();
			var exception = new Exception("test");
			actorRef.Send(new Status.Failure(exception), sender);

			A.CallTo(() => promise.TryFailure(exception)).MustHaveHappened();
		}

		[Fact]
		public void When_timing_out_Then_AskTimeoutException_is_thrown()
		{
			var actorSystem = ActorSystem.Create();
			var promiseActorRef = PromiseActorRef.Create(actorSystem, 10, "target");
			Assert.Throws<AggregateException>(() => promiseActorRef.Future.Wait()).ContainsException<AskTimeoutException>().Should().BeTrue();
		}

		[Fact]
		public void Given_a_timed_out_instance_When_sending_messages_to_it_Then_messages_are_forwarded_to_deadLetter()
		{
			var deadLetterActor = A.Fake<ActorRef>();
			DefaultActorSystemFactory.Instance.DeadLetterActorCreator = path => deadLetterActor;

			var actorSystem = ActorSystem.Create();
			var promiseActorRef = PromiseActorRef.Create(actorSystem, 10, "target");
			try{promiseActorRef.Future.Wait();}
			catch(Exception){}

			var fakeSender = A.Fake<ActorRef>();
			var message1 = new object();
			var message2 = new object();
			promiseActorRef.Send(message1,fakeSender);
			promiseActorRef.Send(message2,fakeSender);

			A.CallTo(() => deadLetterActor.Send(message1, fakeSender)).MustHaveHappened(Repeated.Exactly.Once);
			A.CallTo(() => deadLetterActor.Send(message2, fakeSender)).MustHaveHappened(Repeated.Exactly.Once);
		}


	}
}