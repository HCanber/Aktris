using System;
using System.Collections.Generic;
using Aktris.Test.TestHelpers;
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
	}
}