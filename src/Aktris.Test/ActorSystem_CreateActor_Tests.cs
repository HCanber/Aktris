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
			var actorRef = system.CreateActor(new DelegateActorFactory(() => new FakeActor()));
			actorRef.Should().BeOfType<LocalActorRef>(null, null);
		}

		[Fact]
		public void Given_an_ActorSystem_when_creating_actors_with_no_names_Then_they_are_assigned_random_different_names()
		{
			var system = ActorSystem.Create();
			var delegateActorFactory = new DelegateActorFactory(() => new FakeActor());
			var actorRef1 = system.CreateActor(delegateActorFactory, name: null);
			var actorRef2 = system.CreateActor(delegateActorFactory, name: null);
			actorRef1.Name.Should().NotBe(actorRef2.Name);
		}
		private class FakeActor : Actor
		{

		}
	}
}