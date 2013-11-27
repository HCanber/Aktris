using System;
using System.Collections.Generic;
using Aktris.Internals;
using Aktris.Internals.Path;
using Aktris.JetBrainsAnnotations;
using FakeItEasy;
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
		public void When_started_Then_Guardians_should_have_been_created()
		{
			var system = new TestActorSystem();
			system.Start();
			system.RootGuardian.Should().NotBeNull();
			system.UserGuardian.Should().NotBeNull();
			system.SystemGuardian.Should().NotBeNull();
		}

		[Fact]
		public void When_creating_actors_in_system_Then_they_should_be_created_in_UserGuardian()
		{
			var system = new ActorSys();
			system.Start();
			system.CreateActor(ActorCreationProperties.Create<TestActor>(), "Actor1");
			system.CreateActor(ActorCreationProperties.Create<TestActor>(), "Actor2");
			system.CreateActor(ActorCreationProperties.Create<TestActor>(), "Actor3");
			system.Children.Should().ContainInOrder(new object[] {"Actor1", "Actor2", "Actor3"});
		}

		private class ActorSys : TestActorSystem
		{
			public List<string> Children=new List<string>();
			protected override InternalActorRef CreateUserGuardian(GuardianActorRef rootGuardian)
			{
				var localActorRef = A.Fake<LocalActorRef>(builder => builder.WithArgumentsForConstructor(() => new LocalActorRef(this,A.Fake<ActorInstantiator>(), new RootActorPath("test"), CreateDefaultMailbox(),A.Fake<InternalActorRef>())));
				A.CallTo(() => localActorRef.CreateActor(A<ActorCreationProperties>.Ignored, A<string>.Ignored)).Invokes(a => Children.Add((string) a.Arguments[1]));
				return localActorRef;
			}
		}
	}
}