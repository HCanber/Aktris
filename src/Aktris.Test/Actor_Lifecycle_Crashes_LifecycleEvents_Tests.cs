using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace Aktris.Test
{
	// ReSharper disable once InconsistentNaming
	public class Actor_Lifecycle_Crashes_LifecycleEvents_Tests
	{
		[Fact]
		public void When_an_actor_restarts_Then_its_lifecycle_events_are_called()
		{
			var system = new TestActorSystem();
			system.Start();
			var actors = new List<TestActor>();
			var props = ActorCreationProperties.Create(() => { actors.Add(new TestActor()); return actors[actors.Count - 1]; });
			var actor = system.CreateActor(props);
			actors[0].Calls.Clear();
			actor.Send("A trigger message that will cause actor to fail", null);
			actors.Should().HaveCount(2);
			actors[0].Calls.Should().ContainInOrder(new[] { "PreRestart", "PostStop" });
			actors[1].Calls.Should().ContainInOrder(new[] { "PreStart", "PostRestart" });
		}


		private class TestActor : Actor
		{
			public List<string> Calls = new List<string>();
			public TestActor()
			{
				ReceiveAny(_ => { throw new Exception(); });
			}

			protected internal override void PreFirstStart()
			{
				Calls.Add("PreFirstStart");
			}

			protected internal override void PreStart()
			{
				Calls.Add("PreStart");
			}

			protected internal override void PreRestart(Exception cause, object message)
			{
				Calls.Add("PreRestart");
			}

			protected internal override void PostStop()
			{
				Calls.Add("PostStop");
			}

			protected internal override void PostRestart(Exception cause)
			{
				Calls.Add("PostRestart");
			}
		}
	}
}