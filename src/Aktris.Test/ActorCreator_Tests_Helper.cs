using System;
using Aktris.Exceptions;
using Aktris.Internals;
using FakeItEasy;
using FluentAssertions;
using Xunit;
using Xunit.Extensions;

namespace Aktris.Test
{
	// ReSharper disable InconsistentNaming
	public abstract class ActorCreator_Tests_Helper
	{
		protected abstract IActorCreator GetActorCreator(IBootstrapper bootstrapper = null);


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

		[Fact]
		public void When_created_actor_Then_start_is_called_on_LocalActorRef()
		{
			var bootstrapper = DefaultActorSystemFactory.Instance;
			var fakeLocalActorRefFactory = A.Fake<LocalActorRefFactory>();
			var fakeActorRef = A.Fake<ILocalActorRef>();
			A.CallTo(() => fakeLocalActorRefFactory.CreateActor(A<ActorFactory>.Ignored, A<string>.Ignored)).Returns(fakeActorRef);
			bootstrapper.LocalActorRefFactory = fakeLocalActorRefFactory;

			var actorCreator = GetActorCreator(bootstrapper);

			var actorRef = actorCreator.CreateActor(new DelegateActorFactory(() => new FakeActor()));

			A.CallTo(() => fakeActorRef.Start()).MustHaveHappened();
		}
	}
	// ReSharper restore InconsistentNaming
}