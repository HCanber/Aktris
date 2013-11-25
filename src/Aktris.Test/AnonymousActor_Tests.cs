using System;
using System.Collections.Generic;
using Aktris.Test.Internals;
using Aktris.Test.TestHelpers;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Aktris.Test
{
	// ReSharper disable once InconsistentNaming
	public class AnonymousActor_Tests
	{
		[Fact]
		public void When_actor_handles_messages_Then_it_calls_the_correct_handler()
		{
			var receivedStrings = new List<string>();
			var receivedInts = new List<int>();
			var receivedFloats = new List<float>();
			var receivedObjects = new List<object>();
			var allRecievedMessages = new List<object>();
			Func<Actor> actorFactory = () => AnonymousActor.Create(c =>
			{
				// ReSharper disable ConvertClosureToMethodGroup
				c.AddReceiver(typeof(object), msg => { allRecievedMessages.Add(msg); return false; });
				c.Receive<float>(f => receivedFloats.Add(f));
				c.Receive<int>(i => receivedInts.Add(i));
				c.ReceiveAny(o => receivedObjects.Add(o));
				c.Receive<string>(s => receivedStrings.Add(s));//Since we have a Catch-all above then no floats should end up here
				// ReSharper restore ConvertClosureToMethodGroup
			});
			var actor = ActorHelper.CreateInitializedActorDirectly(actorFactory);
			actor.HandleMessage(1.0f);
			actor.HandleMessage(2);
			actor.HandleMessage(true);
			actor.HandleMessage("4");

			receivedFloats.Should().BeEquivalentTo(1.0f);
			receivedInts.Should().BeEquivalentTo(2);
			receivedObjects.Should().BeEquivalentTo(true, "4");
			receivedStrings.Should().BeEmpty();
			allRecievedMessages.Should().BeEquivalentTo(1.0f, 2, true, "4");
		}

		[Fact]
		public void When_actor_forwards_messages_of_specific_types_Then_it_calls_send_on_receiving_actor()
		{
			var testActorSystem = new TestActorSystem();
			TestableActor recipientActor = null;
			var recipient = testActorSystem.CreateActor(ActorCreationProperties.Create(() => { recipientActor = new TestableActor(); return recipientActor; }));
			var receivedObjects = new List<object>();
			var sut = testActorSystem.CreateActor(ActorCreationProperties.Create(() => AnonymousActor.Create(c =>
			{
				// ReSharper disable ConvertClosureToMethodGroup
				c.ReceiveAndForward<int>(recipient);
				c.ReceiveAndForward<float>(recipient);
				c.ReceiveAny(o => receivedObjects.Add(o));
				// ReSharper restore ConvertClosureToMethodGroup
			})));

			var senderActor = testActorSystem.CreateActor(ActorCreationProperties.Create(() => new SendingActor(sut, 1, "2", 3.0f)));

			senderActor.Send("Send 1 2 and 3.0", null);
			recipientActor.ReceivedMessages.Should().HaveCount(2);
			recipientActor.ReceivedMessages[0].Item1.Should().BeSameAs(senderActor);
			recipientActor.ReceivedMessages[0].Item2.Should().Be(1);
			recipientActor.ReceivedMessages[1].Item1.Should().BeSameAs(senderActor);
			recipientActor.ReceivedMessages[1].Item2.Should().Be(3.0f);
			receivedObjects.Should().BeEquivalentTo(new object[] { "2" });
		}

		private class SendingActor : Actor
		{
			public SendingActor(ActorRef recipient, params object[] messages)
			{
				ReceiveAny(_ =>
				{
					foreach(var message in messages)
					{
						recipient.Send(message, Self);
					}
				});
			}
		}
	}
}