using System;
using System.Collections.Generic;
using System.Linq;
using Aktris.Dispatching;
using Aktris.Internals;
using Aktris.Internals.Helpers;
using Aktris.Internals.Path;
using Aktris.Internals.SystemMessages;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Aktris.Test
{
// ReSharper disable once InconsistentNaming
	public class Actor_Lifecycle_Crashes_Tests
	{
		[Fact]
		public void When_an_actor_throws_exception_during_handling_message_Then_Failed_message_is_sent_to_parent()
		{
			var supervisor = A.Fake<InternalActorRef>();
			var child = new LocalActorRef(new TestActorSystem(), ActorCreationProperties.CreateAnonymous(c => c.ReceiveAny(_ => { throw new Exception("Child failed"); })), new RootActorPath("child"), new UnboundedMailbox(new SynchronousScheduler()), supervisor);
			child.Start();
			child.Send("A trigger message that will cause Child to fail", null);
			A.CallTo(() => supervisor.HandleSystemMessage(A<SystemMessageEnvelope>.That.Matches(e => e.Message is ActorFailed))).MustHaveHappened();
		}


		[Fact]
		public void When_an_actor_throws_exception_during_handling_message_Then_the_actors_mailbox_gets_suspended()
		{
			var system = new TestActorSystem();
			system.Start();

			var mailbox = new TestMailbox(system.CreateDefaultMailbox());
			var props = new DelegateActorCreationProperties(() => AnonymousActor.Create<object>(_ => { throw new Exception(); }))
			{
				MailboxCreator = () => mailbox
			};

			var actor = system.CreateActor(props);
			actor.Send("A trigger message that will cause actor to fail", null);
			var suspendCalls = mailbox.GetStateChangesFor(TestMailbox.StateChange.Suspend);
			suspendCalls.Count.Should().Be(1);
		}

		[Fact]
		public void When_an_actor_throws_exception_during_handling_message_Then_its_children_gets_suspended()
		{
			var system = new TestActorSystem();
			system.Start();

			var childrenMailboxes = Enumerable.Range(1, 10).Select(_ => new TestMailbox(system.CreateDefaultMailbox())).ToList();


			var parent = system.CreateActor(ActorCreationProperties.Create(() => new ParentWhichFailsWithChildrenActor(childrenMailboxes)));
			parent.Send("A trigger message that will cause the parent actor to fail", null);
			var childSuspends = childrenMailboxes.Select(m=>m.GetStateChangesFor(TestMailbox.StateChange.Suspend)).ToList();
			for(int i = 0; i < childSuspends.Count; i++)
			{
				var suspendCalls= childSuspends[i];
				suspendCalls.Count.Should().Be(1,"Mailbox for child "+i+" should have been suspended.");
			}
		}

		private class ParentWhichFailsWithChildrenActor : Actor
		{
			public ParentWhichFailsWithChildrenActor(IEnumerable<Mailbox> childrenMailboxes)
			{
				Func<Mailbox, ActorCreationProperties> createChild = m => new DelegateActorCreationProperties(() => new SiblingActor())
				{
					MailboxCreator = () => m
				};
				childrenMailboxes.ForEach((m, i) => CreateActor(createChild(m), "Child" + i));
				ReceiveAny(_=>{throw new Exception();});
			}

			private class SiblingActor : Actor { }
		}
		private class ParentWithFailingChildActor : Actor
		{
			public ParentWithFailingChildActor(Mailbox failingChildMailbox, IEnumerable<Mailbox> siblingMailboxes )
			{
				var failingChildProps = new DelegateActorCreationProperties(() => AnonymousActor.Create<object>(_ => { throw new Exception(); }))
				{
					MailboxCreator = () => failingChildMailbox
				};
				Func<Mailbox,ActorCreationProperties> createSibling =m=> new DelegateActorCreationProperties(() =>new SiblingActor())
				{
					MailboxCreator = () => m
				};

				var failingChild = CreateActor(failingChildProps, "FailingChild");
				siblingMailboxes.ForEach((m, i) => CreateActor(createSibling(m), "Sibling" + i));
				ReceiveAnyAndForward(failingChild);
			}

			private class SiblingActor : Actor { }
		}
	}
}