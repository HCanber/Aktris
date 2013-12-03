using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Aktris.Dispatching;
using Aktris.Exceptions;
using Aktris.Internals;
using Aktris.Internals.Path;
using Aktris.Internals.SystemMessages;
using Aktris.Test.TestHelpers;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Aktris.Test.Internals
{
// ReSharper disable InconsistentNaming
	public class LocalActorRef_Tests
	{

		[Fact]
		public void When_creating_Then_CreateActor_message_is_enqueued_in_mailbox()
		{
			var mailbox = A.Fake<Mailbox>();
			var actorRef = new LocalActorRef(new TestActorSystem(), A.Fake<ActorCreationProperties>(), new RootActorPath("test"), mailbox);

			A.CallTo(() => mailbox.EnqueueSystemMessage(A<SystemMessageEnvelope>.That.Matches(e => e.Message is CreateActor))).MustHaveHappened();
		}


		[Fact]
		public void When_calling_Start_Then_the_actor_is_attached_to_the_mailbox()
		{
			var mailbox = A.Fake<Mailbox>();
			var actorRef = new LocalActorRef(new TestActorSystem(), A.Fake<ActorCreationProperties>(), new RootActorPath("test"), mailbox);

			actorRef.Start();

			A.CallTo(() => mailbox.SetActor(actorRef)).MustHaveHappened();
		}



		[Fact]
		public void When_sending_Then_the_message_is_wrapped_in_an_envelope_and_enqueued_on_the_mailbox()
		{
			var mailbox = A.Fake<Mailbox>();
			var messages = new List<Envelope>();
			A.CallTo(() => mailbox.Enqueue(A<Envelope>.Ignored)).Invokes(x => messages.Add(x.GetArgument<Envelope>(0)));

			var actorRef = new LocalActorRef(new TestActorSystem(), A.Fake<ActorCreationProperties>(), new RootActorPath("test"), mailbox);

			actorRef.Start();
			actorRef.Send("MyTestMessage", null);
			messages.Should().Contain(e => e.Message is string && ((string)e.Message) == "MyTestMessage");
		}

		[Fact]
		public void When_handling_non_existing_SystemMessage_then_Exception_is_thrown()
		{
			var mailbox = A.Fake<Mailbox>();
			var actorInstantiator = A.Fake<ActorInstantiator>();
			var actorRef = new LocalActorRef(new TestActorSystem(), actorInstantiator, new RootActorPath("test"), mailbox);
			Assert.Throws<InvalidOperationException>(() => actorRef.HandleSystemMessage(new SystemMessageEnvelope(actorRef, new NonExistingSystemMessage(), A.Fake<ActorRef>())));
		}

		[Fact]
		public void When_handling_CreateActor_message_and_the_ActorInstantiator_returns_null_then_Exception_is_thrown()
		{
			var mailbox = A.Fake<Mailbox>();
			var actorInstantiator = A.Fake<ActorInstantiator>();
			A.CallTo(() => actorInstantiator.CreateNewActor()).Returns(null);
			var actorRef = new LocalActorRef(new TestActorSystem(), actorInstantiator, new RootActorPath("test"), mailbox);
			Assert.Throws<ActorInitializationException>(()=>actorRef.HandleSystemMessage(new SystemMessageEnvelope(actorRef, new CreateActor(), A.Fake<ActorRef>())));
		}

		[Fact]
		public void When_handling_CreateActor_message_Then_a_new_instance_of_the_actor_is_created()
		{
			var mailbox = A.Fake<Mailbox>();
			var actor = ActorHelper.CreateActorDirectly<TestActor>();
			var actorInstantiator = A.Fake<ActorInstantiator>();
			//Note: NEVER do this in actual code (returning a premade instance). Always create new instances.
			A.CallTo(() => actorInstantiator.CreateNewActor()).Returns(actor);
			var actorRef = new LocalActorRef(new TestActorSystem(), actorInstantiator, new RootActorPath("test"), mailbox);
			actorRef.HandleSystemMessage(new SystemMessageEnvelope(actorRef, new CreateActor(), A.Fake<ActorRef>()));

			A.CallTo(() => actorInstantiator.CreateNewActor()).MustHaveHappened(Repeated.Exactly.Once);
		}

		[Fact]
		public void When_handling_CreateActor_message_Then_Init_is_called_on_the_new_actor()
		{
			var mailbox = A.Fake<Mailbox>();
			Actor actor = null;
			var actorInstantiator = A.Fake<ActorInstantiator>();
			//Note: NEVER do this in actual code (returning a premade instance). Always create new instances.
			A.CallTo(() => actorInstantiator.CreateNewActor()).ReturnsLazily(()=> { actor = A.Fake<Actor>();return actor;});
			var actorRef = new LocalActorRef(new TestActorSystem(), actorInstantiator, new RootActorPath("test"), mailbox);
			actorRef.HandleSystemMessage(new SystemMessageEnvelope(actorRef, new CreateActor(), A.Fake<ActorRef>()));
			A.CallTo(()=>actor.Init()).MustHaveHappened(Repeated.Exactly.Once);
		}

		[Fact]
		public void When_handling_CreateActor_message_Then_the_LocalActorRef_is_pushed_to_stack_and_afterwards_removed()
		{
			var mailbox = A.Fake<Mailbox>();
			var actorInstantiator = A.Fake<ActorInstantiator>();
			ImmutableStack<LocalActorRef> stackDuringActorCreation=null;
			A.CallTo(() => actorInstantiator.CreateNewActor()).Invokes(() =>
			{
				stackDuringActorCreation = ActorHelper.GetActorRefStack();
			}).ReturnsLazily(()=>new TestActor());
			var actorRef = new LocalActorRef(new TestActorSystem(), actorInstantiator, new RootActorPath("test"), mailbox);
			actorRef.HandleSystemMessage(new SystemMessageEnvelope(actorRef, new CreateActor(), A.Fake<ActorRef>()));
			var stackAfterActorCreation = ActorHelper.GetActorRefStack();

			stackDuringActorCreation.IsEmpty.Should().BeFalse("The stack should contain one item");
			stackDuringActorCreation.Peek().Should().BeSameAs(actorRef,"The item on stack should be the LocalActorRef that creates the actor");
			stackDuringActorCreation.Count().Should().Be(1,"The stack should only contain one item.");
			(stackAfterActorCreation == null || stackAfterActorCreation.IsEmpty).Should().BeTrue("The stack should be empty after creation");
		}

		[Fact]
		public void When_handling_message_Then_it_is_forwarded_to_the_actor_and_sender_is_set()
		{
			var mailbox = A.Fake<Mailbox>();
			var actor = ActorHelper.CreateActorDirectly<TestActor>();
			var actorInstantiator = A.Fake<ActorInstantiator>();
			//Note: NEVER do this in actual code (returning a premade instance). Always create new instances.
			A.CallTo(() => actorInstantiator.CreateNewActor()).Returns(actor);
			var actorRef = new LocalActorRef(new TestActorSystem(), actorInstantiator, new RootActorPath("test"), mailbox);
			var message=new object();
			var sender = A.Fake<ActorRef>();
			A.CallTo(() => sender.Name).Returns("SenderActor");

			//Send Create message so that the instance is created
			actorRef.HandleSystemMessage(new SystemMessageEnvelope(actorRef, new CreateActor(), A.Fake<ActorRef>()));


			actorRef.HandleMessage(new Envelope(actorRef,message,sender));

			actor.ReceivedMessages.Should().HaveCount(1);
			actor.ReceivedMessages[0].Item2.Should().BeSameAs(message);
			actor.ReceivedMessages[0].Item1.Name.Should().Be("SenderActor");
		}

		private class NonExistingSystemMessage : SystemMessage { }
	}
	// ReSharper restore InconsistentNaming
}