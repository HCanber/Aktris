using System;
using System.Collections.Generic;
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
			var child = system.CreateActor(ActorCreationProperties.Create(() => { prestartActor = new PrestartActor(); return prestartActor; }));
			child.Send("A message", null);
			prestartActor.PrestartCalledFirst.Should().BeTrue();
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
		public void When_an_actor_throws_exception_during_handling_message_Then_the_actors_mailbox_gets_suspended2()
		{
			var system = new TestActorSystem();
			system.Start();

			var mailbox = new TestMailbox(system.CreateDefaultMailbox());
			var props = new DelegateActorCreationProperties(() => AnonymousActor.Create<object>(_=>{throw new Exception();}))
			{
				MailboxCreator = ()=>mailbox
			};

			var actor = system.CreateActor(props);
			actor.Send("A trigger message that will cause actor to fail", null);
			mailbox.NumberOfSuspendCalls.Should().Be(1);
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
				public bool IsSuspended { get { return ((MailboxBase) InternalSelf.Mailbox).Status.IsSuspended(); } }
			}
		}

		public class TestMailbox : Mailbox
		{
			private readonly Mailbox _mailbox;
			public int NumberOfSuspendCalls { get; private set; }
			public int NumberOfResumeCalls { get; private set; }
			public List<Envelope> EnquedMessages { get; private set; }
			public List<SystemMessageEnvelope> EnquedSystemMessages { get; private set; }

			public TestMailbox(Mailbox mailbox)
			{
				_mailbox = mailbox;
				EnquedMessages=new List<Envelope>();
				EnquedSystemMessages=new List<SystemMessageEnvelope>();
			}

			void Mailbox.SetActor(InternalActorRef actor)
			{
				_mailbox.SetActor(actor);
			}

			void Mailbox.Enqueue(Envelope envelope)
			{
				EnquedMessages.Add(envelope);
				_mailbox.Enqueue(envelope);
			}

			void Mailbox.EnqueueSystemMessage(SystemMessageEnvelope envelope)
			{
				EnquedSystemMessages.Add(envelope);
				_mailbox.EnqueueSystemMessage(envelope);
			}

			void Mailbox.Suspend(InternalActorRef actor)
			{
				NumberOfSuspendCalls++;
				_mailbox.Suspend(actor);
			}

			void Mailbox.Resume(InternalActorRef actor)
			{
				NumberOfResumeCalls++;
				_mailbox.Resume(actor);
			}
		}
	}


}