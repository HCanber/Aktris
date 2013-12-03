using System;
using System.Threading;
using Aktris.Dispatching;
using Aktris.Internals;
using Aktris.Internals.Path;
using Aktris.Internals.SystemMessages;
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
		public void When_an_actor_throws_exception_during_handling_message_Then_the_actor_gets_suspended()
		{
			var system = new TestActorSystem();
			system.Start();
			ParentActor parentActor = null;
			var parent = system.CreateActor(ActorCreationProperties.Create(()=>{parentActor=new ParentActor();return parentActor;}));
			parent.Send("A trigger message that will cause Child to fail", null);
			parentActor.Child.IsSuspended.Should().BeTrue();

			throw new Exception("Incomplete");
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

		private class ParentActor : Actor
		{
			public ChildActor Child;
			public ParentActor()
			{
				var child = CreateActor(ActorCreationProperties.Create(()=>{Child=new ChildActor();return Child;}));
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
	}
}