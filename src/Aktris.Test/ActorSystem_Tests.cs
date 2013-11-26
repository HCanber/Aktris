using System;
using Aktris.Internals;
using Xunit;

namespace Aktris.Test
{
// ReSharper disable once InconsistentNaming
	public class ActorSystem_Tests
	{
		[Fact]
		public void Given_a_system_that_has_not_been_started_When_creating_an_actor_Then_it_should_fail()
		{
			var system = new InternalActorSystem("default", new TestBootstrapper());
			Assert.Throws<InvalidOperationException>(() => system.CreateActor(ActorCreationProperties.CreateAnonymous(c => { })));
		}
	}
}