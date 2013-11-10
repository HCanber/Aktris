using Aktris.Internals;
using FluentAssertions;
using Xunit;

namespace Aktris.Test
{
	public class ActorSystem_CreateActor_Tests
	{
		[Fact]
		public void Given_an_ActorSystem_we_can_create_an_local_actor()
		{
			var system = ActorSystem.Create();
			var actorRef = system.CreateActor();
			actorRef.Should().BeOfType<LocalActorRef>(null, null);
		}
	}
}