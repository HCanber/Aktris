using System;
using System.Threading;
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
			PrestartActor prestartActor=null;
			var child = system.CreateActor(ActorCreationProperties.Create(()=> { prestartActor = new PrestartActor();return prestartActor;}));
			child.Send("A message",null);
			prestartActor.PrestartCalledFirst.Should().BeTrue();
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
	}
}