using System;
using System.Collections.Immutable;
using System.Linq;
using Aktris.Dispatching;
using Aktris.Exceptions;
using Aktris.Internals;
using Aktris.Internals.SystemMessages;
using Aktris.Test.Internals;
using Aktris.Test.TestHelpers;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Aktris.Test
{
	public class Actor_Tests
	{
		[Fact]
		public void Creating_an_actor_directly_should_fail()
		{
			Assert.Throws<InvalidOperationException>(() => new TestActor());
		}

		[Fact]
		public void When_Actor_is_created_Then_it_marks_the_ActorRef_as_consumed()
		{
			var mailbox = A.Fake<Mailbox>();
			var actorInstantiator = A.Fake<ActorInstantiator>();
			TestActor actor = null;
			ImmutableStack<LocalActorRef> stackDuringActorCreation = null;
			A.CallTo(() => actorInstantiator.CreateNewActor()).Invokes(() =>
			{
				stackDuringActorCreation = ActorHelper.GetActorRefStack();
			}).ReturnsLazily(() =>
			{
				actor = new TestActor();
				return actor;
			});

			var actorRef = new LocalActorRef(actorInstantiator, "test", mailbox);

			//Send Create message so that the instance is created
			actorRef.HandleSystemMessage(new SystemMessageEnvelope(actorRef, new CreateActor(), A.Fake<ActorRef>()));

			var actorRefsAddedDuringConstructor = actor.LocalActorRefStackInConstructor.Except(stackDuringActorCreation).ToList();
			actorRefsAddedDuringConstructor.Count.Should().Be(1);
			actorRefsAddedDuringConstructor[0].Should().BeNull();
		}


		private class TestActor : Actor
		{
			public TestActor()
			{
				//At this stage base ctor has been called.
				//Store what's on the stack right now.
				LocalActorRefStackInConstructor = ActorHelper.GetActorRefStack();
			}

			public ImmutableStack<LocalActorRef> LocalActorRefStackInConstructor { get; set; }
		}
	}
}