using System;
using System.Linq;
using Aktris.Internals.SystemMessages;
using FluentAssertions;
using Xunit;

namespace Aktris.Test
{
	// ReSharper disable once InconsistentNaming
	public class Actor_Lifecycle_Watch_Tests
	{
		[Fact]
		public void When_watching_another_actor_Then_a_Watch_message_is_sent_to_that_actor()
		{
			var system = new TestActorSystem();
			system.Start();

			var mailbox = new TestMailbox(system.CreateDefaultMailbox());
			var watchedActorProps = new DelegateActorCreationProperties(() => AnonymousActor.Create<object>(_ => { }))
			{
				MailboxCreator = () => mailbox
			};
			var watchedActor = system.CreateActor(watchedActorProps,"WatchedActor");
			var watcher = system.CreateActor(ActorCreationProperties.Create(() => new WatchingActor(watchedActor)), "Watcher");
			var watchMessages = mailbox.GetEnquedSystemMessagesOfType<WatchActor>();
			watchMessages.Should().HaveCount(1);
			watchMessages[0].Watcher.Should().BeSameAs(watcher);
		}

		private class WatchingActor : Actor
		{
			public WatchingActor(ActorRef actorToWatch)
			{
				Watch(actorToWatch);
			}
		}
	}
}