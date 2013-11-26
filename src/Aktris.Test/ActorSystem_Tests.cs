using System;
using Aktris.Internals;
using Aktris.JetBrainsAnnotations;
using FluentAssertions;
using Xunit;

namespace Aktris.Test
{
// ReSharper disable once InconsistentNaming
	public class ActorSystem_Tests
	{
		[Fact]
		public void Given_a_system_that_has_not_been_started_When_creating_an_actor_Then_it_should_fail()
		{
			var system = new TestActorSystem();
			Assert.Throws<InvalidOperationException>(() => system.CreateActor(ActorCreationProperties.CreateAnonymous(c => { })));
		}

		[Fact]
		public void When_started_Then_a_Guardian_should_have_been_created()
		{
			var system = new TestActorSystem();
			system.Start();
			system.RootGuardian.Should().NotBeNull();
		}

	}
}