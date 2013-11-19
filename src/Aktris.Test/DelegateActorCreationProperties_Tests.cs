using System;
using Aktris.Test.TestHelpers;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Aktris.Test
{
	public class DelegateActorCreationProperties_Tests
	{
		[Fact]
		public void When_creating_with_null_argument_Then_exception_is_thrown()
		{
			// ReSharper disable once AssignNullToNotNullAttribute
			Assert.Throws<ArgumentNullException>(() => new DelegateActorCreationProperties(null));
		}

		[Fact]
		public void Given_a_DelegateActorFactory_When_creating_a_new_actor_Then_the_supplied_function_is_called()
		{
			var functionWasCalled = false;
			var factory = new DelegateActorCreationProperties(() => { functionWasCalled = true; return null; });

			factory.CreateNewActor();

			functionWasCalled.Should().BeTrue();
		}


		[Fact]
		public void Given_a_factory_created_by_ActorFactory_When_creating_actors_Then_they_are_different()
		{
			var factory = new DelegateActorCreationProperties(ActorHelper.CreateActorDirectly(()=>new Test.FakeActor()));

			var actor1 = factory.CreateNewActor();
			var actor2 = factory.CreateNewActor();

			actor1.Should().NotBeSameAs(actor2);
		}

	}
}