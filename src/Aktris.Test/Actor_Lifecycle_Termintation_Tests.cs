using System;
using System.Collections.Generic;
using System.Linq;
using Aktris.Dispatching;
using Aktris.Internals.Helpers;
using Aktris.Internals.SystemMessages;
using Aktris.Supervision;
using Aktris.Test.TestHelpers;
using FluentAssertions;
using Xunit;

namespace Aktris.Test
{
// ReSharper disable once InconsistentNaming
	public class Actor_Lifecycle_Termintation_Tests
	{
		[Fact]
		public void When_actor_Stops_Then_it_sends_ActorTerminated_message_to_supervisor()
		{
			var system = new TestActorSystem();
			system.Start();
			var parentMailbox = new TestMailbox(system.CreateDefaultMailbox());
			ParentWithFailingChildActor parentInstance = null;
			var props = new DelegateActorCreationProperties(() =>parentInstance=new ParentWithFailingChildActor())
			{
				MailboxCreator = () => parentMailbox
			};
			var actor = system.CreateActor(props,"Parent");
			actor.Send("A trigger message that will cause actor to fail", null);
			var actorsTerminated = parentMailbox.GetStateChangesForEnquingSystemMessagesOfType<ActorTerminated>().ToList();
			actorsTerminated.Should().HaveCount(1).And.ContainSingle(s => ((ActorTerminated) s.GetLastEnqueuedSystemMessage().Message).TerminatedActor == parentInstance.FailingChild);
		}

		private class ParentWithFailingChildActor : Actor
		{
			private readonly SupervisorStrategy _supervisorStrategy;
			private ActorRef _failingChild;

			public ParentWithFailingChildActor(SupervisorStrategy supervisorStrategy = null)
			{
				_supervisorStrategy = supervisorStrategy;

				_failingChild = CreateActor(ActorCreationProperties.Create<StoppingActor>(), "FailingChild");
				ReceiveAnyAndForward(_failingChild);
			}

			protected override SupervisorStrategy SupervisorStrategy { get { return _supervisorStrategy; } }
			public ActorRef FailingChild {get { return _failingChild; }}
		}

		public class StoppingActor : Actor
		{
			public StoppingActor()
			{
				ReceiveAny(_=> Stop());
			}
		}
	}
}