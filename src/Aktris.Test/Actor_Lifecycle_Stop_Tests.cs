using System;
using System.Collections.Generic;
using Aktris.Messages;
using Aktris.Test.TestHelpers;
using FluentAssertions;
using Xunit;

namespace Aktris.Test
{
	// ReSharper disable once InconsistentNaming
	public class Actor_Lifecycle_Stop_Tests
	{
		[Fact]
		public void When_actor_receives_StopActor_message_Then_it_terminates()
		{
			var system = new TestActorSystem();
			system.Start();
			var actor = system.CreateActor(()=>new TestActor(system),"Parent");
			actor.Send("stopChild", null);
			actor.Actor.ReceivedTerminate.Should().BeTrue();
		}

		[Fact]
		public void When_actor_receives_StopActor_it_process_messages_prior_to_StopActor_message()
		{
			var system = new TestActorSystem();
			system.Start();
			var actor = system.CreateActor(() => new TestActor(system, childMailboxShouldBeOpen:false),"Parent");
			actor.Send("1", null);
			actor.Send("2", null);
			actor.Send("3", null);
			actor.Send("stopChild", null);
			actor.Send("4", null);
			actor.Send("openChildMailbox", null);

			actor.Actor.Replies.Should().ContainInOrder(new[] { "1", "2", "3" });
		}

		private class TestActor : Actor
		{
			public bool ReceivedTerminate { get; private set; }
			public List<string> Replies { get; private set; }
			public TestActor(ActorSystem system, bool childMailboxShouldBeOpen=true)
			{
				Replies = new List<string>();
				var childMailbox = new DelayedTestMailbox(system.CreateDefaultMailbox());
				var child = CreateActor(new DelegateActorCreationProperties(() => AnonymousActor.Create(c=>c.ReceiveAny((msg,sender)=>sender.Reply(msg)))) { MailboxCreator = () => childMailbox },"Child");
				if(childMailboxShouldBeOpen)
					childMailbox.Open();

				Watch(child);

				Receive<WatchedActorTerminated>(terminated => ReceivedTerminate = true, m => m.TerminatedActor == child);	//Record that child terminated
				Receive<string>(_ => child.Send(StopActor.Instance, Self), s => s == "stopChild"); //Send Stop to child when any other message is received
				Receive<string>(_ => childMailbox.Open(), s => s == "openChildMailbox");
				Receive<string>(s => Replies.Add(s), _ => Sender.Path == child.Path);	//Record replies from child
				ReceiveAny(m=>child.Send(m,Self));	//Forward all other messages
			}
		}

		private class ReplyingActor : Actor
		{
			public ReplyingActor()
			{
				ReceiveAny(msg => Sender.Reply(msg));
			}
		}
	}
}