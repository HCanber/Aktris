using FluentAssertions;
using Xunit;

namespace Aktris.Test
{
// ReSharper disable InconsistentNaming
	public abstract class ActorCreator_Tests_Helper
	{
		protected abstract IActorCreator GetActorCreator();


		[Fact]
		public void Given_an_ActorCreator_when_creating_actors_with_no_names_Then_they_are_assigned_random_different_names()
		{
			var delegateActorFactory = new DelegateActorFactory(() => new FakeActor());
			var actorCreator = GetActorCreator();
			var actorRef1 = actorCreator.CreateActor(delegateActorFactory, name: null);
			var actorRef2 = actorCreator.CreateActor(delegateActorFactory, name: null);
			actorRef1.Name.Should().NotBe(actorRef2.Name);
		}
	}
	// ReSharper restore InconsistentNaming
}