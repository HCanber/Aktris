using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Aktris.Dispatching;
using Aktris.Internals;
using Aktris.Internals.Helpers;
using Aktris.Internals.Path;
using Aktris.Internals.SystemMessages;
using Aktris.JetBrainsAnnotations;
using Aktris.Test.TestHelpers;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Aktris.Test
{
	public class Actor_Lifecycle_Tests
	{
		[Fact]
		public void When_an_actor_is_created_Then_its_prestart_is_called_before_messages_are_processed()
		{
			var system = new TestActorSystem();
			system.Start();
			PrestartActor prestartActor = null;
			var child = system.CreateActor(ActorCreationProperties.Create(() =>
			{
				prestartActor = new PrestartActor();
				return prestartActor;
			}));
			child.Send("A message", null);
			prestartActor.PrestartCalledFirst.Should().BeTrue();
		}

		[Fact]
		public void When_a_child_actor_is_created_Then_it_sends_SuperviseActor_message_to_parent()
		{
			var system = new TestActorSystem();
			system.Start();

			var mailbox = new TestMailbox(system.CreateDefaultMailbox());
			ParentWithChildActor parent=null;
			var parentProps = new DelegateActorCreationProperties(() =>{parent=new ParentWithChildActor();return parent;})
			{
				MailboxCreator = () => mailbox
			};

			var parentRef = system.CreateActor(parentProps,"Parent");
			var stateChanges = mailbox.GetStateChangesForEnquingSystemMessagesOfType<SuperviseActor>();
			stateChanges.Count.Should().Be(1);
			((SuperviseActor) stateChanges.First().LastEnqueuedSystemMessage.Message).ActorToSupervise.Should().BeSameAs(parent.Child);
		}

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
			parent.Send("A trigger message that will cause a child actor to fail", null);
			var childSuspends = childrenMailboxes.Select(m=>m.GetStateChangesFor(TestMailbox.StateChange.Suspend)).ToList();
			for(int i = 0; i < childSuspends.Count; i++)
			{
				var suspendCalls= childSuspends[i];
				suspendCalls.Count.Should().Be(1,"Mailbox for child "+i+" should have been suspended.");
			}
		}

		private class PrestartActor : Actor
		{
			public bool? PrestartCalledFirst;

			public PrestartActor()
			{
				ReceiveAny(_ => { if(!PrestartCalledFirst.HasValue) PrestartCalledFirst = false; });
			}

			protected internal override void PreStart()
			{
				if(!PrestartCalledFirst.HasValue) PrestartCalledFirst = true;
			}
		}

		private class ParentWithChildActor : Actor
		{
			public ParentWithChildActor()
			{
				Child = CreateActor<ChildActor>("Child");
			}

			public ActorRef Child { get; private set; }

			public class ChildActor : Actor{}
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