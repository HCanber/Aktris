using Aktris.Exceptions;
using FluentAssertions;
using Xunit;
using Xunit.Extensions;

namespace Aktris.Test
{
// ReSharper disable InconsistentNaming
	public abstract class ActorCreator_Tests_Helper
	{
		protected abstract IActorCreator GetActorCreator();


		[Fact]
		public void Given_an_ActorCreator_When_creating_actors_with_no_names_Then_they_are_assigned_random_different_names()
		{
			var delegateActorFactory = new DelegateActorFactory(() => new FakeActor());
			var actorCreator = GetActorCreator();
			var actorRef1 = actorCreator.CreateActor(delegateActorFactory, name: null);
			var actorRef2 = actorCreator.CreateActor(delegateActorFactory, name: null);
			actorRef1.Name.Should().NotBe(actorRef2.Name);
		}


		[Theory]
		[InlineData("")]
		[InlineData("_InvalidFirstCharacter")]
		[InlineData("$InvalidFirstCharacter")]
		[InlineData("Space in name")]
		[InlineData("ÅÄÖNonAsciiCharacters")]
		public void Given_an_ActorCreator_When_creating_actors_with_invalid_name_Then_it_fails(string name)
		{
			var delegateActorFactory = new DelegateActorFactory(() => new FakeActor());
			var actorCreator = GetActorCreator();
			Assert.Throws<InvalidActorNameException>(() => actorCreator.CreateActor(delegateActorFactory, name: name));
		}


		[Theory]
		[InlineData("a")]
		[InlineData("7")]
		[InlineData("X")]
		[InlineData("testing-characters-_=+,.!~")]
		public void Given_an_ActorCreator_When_creating_actors_with_valid_name_Then_it_succeeds(string name)
		{
			var delegateActorFactory = new DelegateActorFactory(() => new FakeActor());
			var actorCreator = GetActorCreator();
			Assert.Throws<InvalidActorNameException>(() => actorCreator.CreateActor(delegateActorFactory, name: name));
		}
	}
	// ReSharper restore InconsistentNaming
}