using FluentAssertions;
using Xunit;

namespace Aktris.Test
{
// ReSharper disable InconsistentNaming
	public class ActorFactory_Tests
	{
		[Fact]
		public void Given_a_factory_created_by_ActorFactory_When_creating_actors_Then_they_are_different()
		{
			var factory = ActorCreationProperties.Create<FakeActor>();

			var actor1 = factory.CreateNewActor();
			var actor2 = factory.CreateNewActor();

			actor1.Should().NotBeSameAs(actor2);
		}

		private class FakeActor : Actor
		{

		}
	}
	// ReSharper restore InconsistentNaming
}