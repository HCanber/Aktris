using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Aktris.Dispatching;
using Aktris.Internals;
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

		private class ParentWithFailingChildActor : Actor
		{
			public ChildActor Child;

			public ParentWithFailingChildActor(Mailbox childMailbox)
			{
				var props = new DelegateActorCreationProperties(() => { Child = new ChildActor(); return Child; })
				{
					MailboxCreator = () => childMailbox
				};

				var child = CreateActor(props, "Child");
				ReceiveAnyAndForward(child);
			}

			public class ChildActor : Actor
			{
				public ChildActor()
				{
					ReceiveAny(_ => { throw new Exception("Child failed"); });
				}

				public bool IsSuspended { get { return ((MailboxBase)InternalSelf.Mailbox).Status.IsSuspended(); } }
			}
		}
	}
}