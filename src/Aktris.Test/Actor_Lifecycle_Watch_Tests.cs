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
			var watchedActor = system.CreateActor(watchedActorProps, "WatchedActor");
			var watcher = system.CreateActor(ActorCreationProperties.Create(() => new WatchingActor(watchedActor)), "Watcher");
			watcher.Send("watch",null);
			var watchMessages = mailbox.GetEnquedSystemMessagesOfType<WatchActor>();
			watchMessages.Should().HaveCount(1);
			watchMessages[0].Watcher.Should().BeSameAs(watcher);
		}

		[Fact]
		public void When_watching_an_actor_more_than_once_Then_no_new_Watch_message_are_sent_to_that_actor()
		{
			var system = new TestActorSystem();
			system.Start();

			var mailbox = new TestMailbox(system.CreateDefaultMailbox());
			var watchedActorProps = new DelegateActorCreationProperties(() => AnonymousActor.Create<object>(_ => { }))
			{
				MailboxCreator = () => mailbox
			};
			var watchedActor = system.CreateActor(watchedActorProps, "WatchedActor");
			var watcher = system.CreateActor(ActorCreationProperties.Create(() => new WatchingActor(watchedActor)), "Watcher");
			watcher.Send("watch", null);
			watcher.Send("watch", null);
			watcher.Send("watch", null);
			var watchMessages = mailbox.GetEnquedSystemMessagesOfType<WatchActor>();
			watchMessages.Should().HaveCount(1);
			watchMessages[0].Watcher.Should().BeSameAs(watcher);
		}

		[Fact]
		public void When_unwatching_an_actor_already_beeing_watched_Then_Unwatch_message_is_sent_to_that_actor()
		{
			var system = new TestActorSystem();
			system.Start();

			var mailbox = new TestMailbox(system.CreateDefaultMailbox());
			var watchedActorProps = new DelegateActorCreationProperties(() => AnonymousActor.Create<object>(_ => { }))
			{
				MailboxCreator = () => mailbox
			};
			var watchedActor = system.CreateActor(watchedActorProps, "WatchedActor");
			var watcher = system.CreateActor(ActorCreationProperties.Create(() => new WatchingActor(watchedActor)), "Watcher");
			watcher.Send("watch", null);

			watcher.Send("unwatch", null);
			var watchMessages = mailbox.GetEnquedSystemMessagesOfType<UnwatchActor>();
			watchMessages.Should().HaveCount(1);
			watchMessages[0].Watcher.Should().BeSameAs(watcher);
		}
		[Fact]
		public void When_rewatching_an_unwatched_actor_Then_Watch_message_is_sent_to_that_actor()
		{
			var system = new TestActorSystem();
			system.Start();

			var mailbox = new TestMailbox(system.CreateDefaultMailbox());
			var watchedActorProps = new DelegateActorCreationProperties(() => AnonymousActor.Create<object>(_ => { }))
			{
				MailboxCreator = () => mailbox
			};
			var watchedActor = system.CreateActor(watchedActorProps, "WatchedActor");
			var watcher = system.CreateActor(ActorCreationProperties.Create(() => new WatchingActor(watchedActor)), "Watcher");
			watcher.Send("watch", null);
			watcher.Send("unwatch", null);
			mailbox.ClearEnqueuedSystemMessages();
			watcher.Send("watch", null);
			var watchMessages = mailbox.GetEnquedSystemMessagesOfType<WatchActor>();
			watchMessages.Should().HaveCount(1);
			watchMessages[0].Watcher.Should().BeSameAs(watcher);
		}


		private class WatchingActor : Actor
		{
			public WatchingActor(ActorRef actorToWatch)
			{
				Receive<string>(_ => Watch(actorToWatch), m => m == "watch");
				Receive<string>(_ => Unwatch(actorToWatch), m => m == "unwatch");
			}
		}
	}
}