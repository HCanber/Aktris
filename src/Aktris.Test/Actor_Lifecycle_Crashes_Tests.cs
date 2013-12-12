﻿using System;
using System.Collections.Generic;
using System.Linq;
using Aktris.Dispatching;
using Aktris.Internals;
using Aktris.Internals.Helpers;
using Aktris.Internals.Path;
using Aktris.Internals.SystemMessages;
using Aktris.Supervision;
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
			A.CallTo(() => supervisor.SendSystemMessage(A<SystemMessage>.That.Matches(m => m is ActorFailed), child)).MustHaveHappened();
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
			var childSuspends = childrenMailboxes.Select(m => m.GetStateChangesFor(TestMailbox.StateChange.Suspend)).ToList();
			childSuspends.Count.Should().Be(10);
			for(int i = 0; i < childSuspends.Count; i++)
			{
				var suspendCalls = childSuspends[i];
				suspendCalls.Count.Should().Be(1, "Mailbox for child " + i + " should have been suspended.");
			}
		}


		[Fact]
		public void Given_supervisor_that_restarts_all_children_When_child_crashes_Then_all_its_siblings_are_restarted()
		{
			var system = new TestActorSystem();
			system.Start();
			var failingChildMailbox = new TestMailbox(system.CreateDefaultMailbox());
			var childrenMailboxes = Enumerable.Range(1, 10).Select(_ => new TestMailbox(system.CreateDefaultMailbox())).ToList();


			var parent = system.CreateActor(ActorCreationProperties.Create(() => new ParentWithFailingChildActor(failingChildMailbox, childrenMailboxes, AllForOneSupervisorStrategy.DefaultAllForOne)));
			parent.Send("A trigger message that will cause the child actor to fail", null);
			var childRestarts = childrenMailboxes.Select(m => m.GetStateChangesForEnquingSystemMessagesOfType<RestartActor>()).ToList();
			childRestarts.Count.Should().Be(10);
			for(int i = 0; i < childRestarts.Count; i++)
			{
				var suspendCalls = childRestarts[i];
				suspendCalls.Count.Should().Be(1, "Mailbox for child " + i + " should have been restarted.");
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
			private readonly SupervisorStrategy _supervisorStrategy;

			public ParentWithFailingChildActor(Mailbox failingChildMailbox, IEnumerable<Mailbox> siblingMailboxes, SupervisorStrategy supervisorStrategy=null )
			{
				_supervisorStrategy = supervisorStrategy;
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

			protected override SupervisorStrategy SupervisorStrategy { get { return _supervisorStrategy; } }

			private class SiblingActor : Actor { }
		}
	}
}