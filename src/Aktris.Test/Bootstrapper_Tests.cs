using Aktris.Internals;
using FluentAssertions;
using Xunit;

namespace Aktris.Test
{
	public class Bootstrapper_Tests
	{

		[Fact]
		public void When_creating_a_ActorSystem_using_static_Create_method_and_null_as_name_then_default_is_used_as_name()
		{
			var system = new Bootstrapper().CreateSystem(null);
			system.Name.Should().BeEquivalentTo("default");
		}

		[Fact]
		public void When_creating_a_ActorSystem_using_static_Create_method_and_no_name_then_default_is_used_as_name()
		{
			var system = new Bootstrapper().CreateSystem();
			system.Name.Should().BeEquivalentTo("default");
		}


		[Fact]
		public void When_creating_a_ActorSystem_Then_an_InternalActorSystem_is_created()
		{
			var system = new Bootstrapper().CreateSystem();
			system.Should().BeOfType<InternalActorSystem>();
		} 
	}
}