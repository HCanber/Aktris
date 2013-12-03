using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Aktris.Dispatching;
using Aktris.Exceptions;
using Aktris.Internals;
using Aktris.Internals.Path;
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

			var actorRef = new LocalActorRef(new TestActorSystem(), actorInstantiator, new RootActorPath("test"), mailbox);

			//Send Create message so that the instance is created
			actorRef.HandleSystemMessage(new SystemMessageEnvelope(actorRef, new CreateActor(), A.Fake<ActorRef>()));

			var actorRefsAddedDuringConstructor = actor.LocalActorRefStackInConstructor.Except(stackDuringActorCreation).ToList();
			actorRefsAddedDuringConstructor.Count.Should().Be(1);
			actorRefsAddedDuringConstructor[0].Should().BeNull();
		}

		[Fact]
		public void Given_a_initialized_actor_Receive_methods_should_not_be_allowed_on_an_actor_outside_its_constructor()
		{
			var actor = ActorHelper.CreateInitializedActorDirectly<ReceiveFailTestActor>();

			Assert.Throws<InvalidOperationException>(() => actor.MakeAddReceiverActionCall());
			Assert.Throws<InvalidOperationException>(() => actor.MakeAddReceiverFuncCall());
			Assert.Throws<InvalidOperationException>(() => actor.MakeReceiveAnyCall());
			Assert.Throws<InvalidOperationException>(() => actor.MakeReceiveCall());
		}

		[Fact]
		public void When_actor_handles_messages_Then_it_calls_the_correct_handler_registered_in_constructor()
		{
			var actor = ActorHelper.CreateInitializedActorDirectly<ReceiveTestActor>();
			actor.HandleMessage(1.0f);
			actor.HandleMessage(2);
			actor.HandleMessage(true);
			actor.HandleMessage("4");

			actor.ReceivedFloats.Should().BeEquivalentTo(1.0f);
			actor.ReceivedInts.Should().BeEquivalentTo(2);
			actor.ReceivedObjects.Should().BeEquivalentTo(true, "4");
			actor.ReceivedStrings.Should().BeEmpty();
			actor.AllRecievedMessages.Should().BeEquivalentTo(1.0f, 2, true, "4");
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

		private class ReceiveFailTestActor : Actor
		{
			public void MakeReceiveCall() { Receive<string>(s => { }); }

			public void MakeReceiveAnyCall() { ReceiveAny(o => { }); }

			public void MakeAddReceiverActionCall() { AddReceiver(typeof(object), o => { }); }

			public void MakeAddReceiverFuncCall() { AddReceiver(typeof(object), o => true); }
		}

		private class ReceiveTestActor : Actor
		{
			public List<string> ReceivedStrings = new List<string>();
			public List<int> ReceivedInts = new List<int>();
			public List<float> ReceivedFloats = new List<float>();
			public List<object> ReceivedObjects = new List<object>();
			public List<object> AllRecievedMessages = new List<object>();

			public ReceiveTestActor()
			{
				AddReceiver(typeof(object), msg => { AllRecievedMessages.Add(msg); return false; });
				Receive<float>(f => ReceivedFloats.Add(f));
				Receive<int>(i => ReceivedInts.Add(i));
				ReceiveAny(o => ReceivedObjects.Add(o));
				Receive<string>(s => ReceivedStrings.Add(s));//Since we have a Catch-all above then no floats should end up here
			}
		}
	}

}