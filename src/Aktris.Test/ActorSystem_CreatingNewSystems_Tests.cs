using System;
using Aktris.Internals;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Aktris.Test
{
// ReSharper disable InconsistentNaming
	public class ActorSystem_CreatingNewSystems_Tests
	{
		[Fact]
		public void When_creating_an_InternalActorSystem_with_null_a_ArgumentNullException_is_thrown()
		{
			// ReSharper disable once AssignNullToNotNullAttribute
			Assert.Throws<ArgumentNullException>(() => new InternalActorSystem(null, A.Fake<IBootstrapper>()));
		}

		[Fact]
		public void When_creating_an_InternalActorSystem_with_EmptyString_as_name_a_ArgumentException_is_thrown()
		{
			Assert.Throws<ArgumentException>(() => new InternalActorSystem("", A.Fake<IBootstrapper>()));
		}

		[Fact]
		public void When_creating_an_InternalActorSystem_with_invalid_characters_in_the_name_a_ArgumentException_is_thrown()
		{
			Assert.Throws<ArgumentException>(() => new InternalActorSystem("Aktör", A.Fake<IBootstrapper>()));
		}

		[Fact]
		public void Given_a_InternalActorSystem_Then_its_name_can_be_retrieved()
		{
			var system = new InternalActorSystem("MySystem", A.Fake<IBootstrapper>());
			system.Name.Should().BeEquivalentTo("MySystem");
		}

		[Fact]
		public void When_creating_a_ActorSystem_using_static_Create_method_and_null_as_name_then_default_is_used_as_name()
		{
			var system = ActorSystem.Create(null);
			system.Name.Should().BeEquivalentTo("default");
		}

		[Fact]
		public void When_creating_a_ActorSystem_using_static_Create_method_and_no_name_then_default_is_used_as_name()
		{
			var system = ActorSystem.Create();
			system.Name.Should().BeEquivalentTo("default");
		}


		[Fact]
		public void When_creating_a_ActorSystem_Then_an_InternalActorSystem_is_created()
		{
			var system = ActorSystem.Create();
			system.Should().BeOfType<InternalActorSystem>();
		}
	}
	// ReSharper restore InconsistentNaming

}