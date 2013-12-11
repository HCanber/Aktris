﻿using System.Collections.Concurrent;
using System.Linq;
using System.Linq;
using System.Threading;
using Aktris.Internals.SystemMessages;
using Aktris.JetBrainsAnnotations;
using Aktris.Test.TestHelpers;
using FluentAssertions;
using Xunit;

namespace Aktris.Test
{
// ReSharper disable once InconsistentNaming
	public class Actor_Lifecycle_Creation_Tests
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
			ParentWithChildActor parent = null;
			var parentProps = new DelegateActorCreationProperties(() =>
			{
				parent = new ParentWithChildActor();
				return parent;
			})
			{
				MailboxCreator = () => mailbox
			};

			var parentRef = system.CreateActor(parentProps, "Parent");
			var stateChanges = mailbox.GetStateChangesForEnquingSystemMessagesOfType<SuperviseActor>();
			stateChanges.Count.Should().Be(1);
			((SuperviseActor) stateChanges.First().LastEnqueuedSystemMessage.Message).ActorToSupervise.Should().BeSameAs(parent.Child);
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

			public class ChildActor : Actor { }
		}

	}
}