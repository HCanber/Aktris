using System;
using System.Collections.Generic;
using System.Linq;
using Aktris.Dispatching;
using Aktris.Internals;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Aktris.Test.Internals
{
// ReSharper disable InconsistentNaming
	public class LocalActorRef_Tests
	{
		[Fact]
		public void When_calling_Start_Then_the_actor_is_attached_to_the_mailbox()
		{
			var mailbox = A.Fake<Mailbox>();
			var actorRef = new LocalActorRef(A.Fake<ActorCreationProperties>(), "test", mailbox);

			actorRef.Start();

			A.CallTo(() => mailbox.Attach(actorRef)).MustHaveHappened();
		}



		[Fact]
		public void When_sending_Then_the_message_is_wrapped_in_an_envelope_and_enqueued_on_the_mailbox()
		{
			var mailbox = A.Fake<Mailbox>();
			var messages = new List<Envelope>();
			A.CallTo(() => mailbox.Enqueue(A<Envelope>.Ignored)).Invokes(x => messages.Add(x.GetArgument<Envelope>(0)));

			var actorRef = new LocalActorRef(A.Fake<ActorCreationProperties>(), "test", mailbox);

			actorRef.Start();
			actorRef.Send("MyTestMessage", null);
			messages.Should().Contain(e => e.Message is string && ((string)e.Message) == "MyTestMessage");
		}
	}
	// ReSharper restore InconsistentNaming
}