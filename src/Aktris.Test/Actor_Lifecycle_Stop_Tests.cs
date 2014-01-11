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
			var actor = system.CreateActor<TestActor>();
			actor.Send("This will trigger a Send to child",null);
			actor.Actor.ReceivedTerminate.Should().BeTrue();
		}

		private class TestActor : Actor
		{
			public bool ReceivedTerminate { get; set; }
			public TestActor()
			{
				var child = CreateActor<NoopActor>();
				Watch(child);
				Receive<WatchedActorTerminated>(terminated => { if(terminated.TerminatedActor == child) ReceivedTerminate = true; });
				ReceiveAny(_ => child.Send(StopActor.Instance, Self)); //Send Stop to child when any other message is received
			}
		}
	}
}