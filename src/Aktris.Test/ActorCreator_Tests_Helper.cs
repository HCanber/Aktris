using System;
using System.Collections.Generic;
using Aktris.Exceptions;
using Aktris.Internals;
using Aktris.JetBrainsAnnotations;
using FakeItEasy;
using FluentAssertions;
using Xunit;
using Xunit.Extensions;

namespace Aktris.Test
{
	// ReSharper disable InconsistentNaming
	public abstract class ActorCreator_Tests_Helper
	{
		protected virtual Tuple<IActorCreator, ActorSystem> GetActorCreator(IBootstrapper bootstrapper = null)
		{
			return GetActorCreator(null, null);
		}

		protected virtual Tuple<IActorCreator, ActorSystem> GetActorCreator(LocalActorRefFactory localActorRefFactory, IBootstrapper bootstrapper = null)
		{
			throw new NotImplementedException("You need to override this method");
		}

		[Fact]
		public void Given_an_ActorCreator_When_creating_actors_with_no_names_Then_they_are_assigned_random_different_names()
		{
			var delegateActorFactory = new DelegateActorCreationProperties(() => new FakeActor());
			var tuple = GetActorCreator();
			var actorCreator = tuple.Item1;
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
			var delegateActorFactory = new DelegateActorCreationProperties(() => new FakeActor());
			var tuple = GetActorCreator();
			var actorCreator = tuple.Item1;
			Assert.Throws<InvalidActorNameException>(() => actorCreator.CreateActor(delegateActorFactory, name: name));
		}


		[Theory]
		[InlineData("a")]
		[InlineData("7")]
		[InlineData("X")]
		[InlineData("testing-characters-_=+,.!~")]
		public void Given_an_ActorCreator_When_creating_actors_with_valid_name_Then_it_succeeds(string name)
		{
			var delegateActorFactory = new DelegateActorCreationProperties(() => new FakeActor());
			var tuple = GetActorCreator();
			var actorCreator = tuple.Item1;
			actorCreator.CreateActor(delegateActorFactory, name: name).Should().NotBeNull();
		}

		[Fact]
		public void When_created_actor_Then_start_is_called_on_LocalActorRef()
		{
			var fakeLocalActorRefFactory = A.Fake<LocalActorRefFactory>();
			var fakeActorRef = A.Fake<ILocalActorRef>();
			var tuple = GetActorCreator(fakeLocalActorRefFactory);
			var actorSystem = tuple.Item2;
			var actorCreator = tuple.Item1;

			var delegateActorCreationProperties = new DelegateActorCreationProperties(() => new FakeActor());
			A.CallTo(() => fakeLocalActorRefFactory.CreateActor(actorSystem, A<ActorCreationProperties>.That.IsSameAs(delegateActorCreationProperties), A<string>.Ignored)).ReturnsLazily(() => { return fakeActorRef; });

			var actorRef = actorCreator.CreateActor(delegateActorCreationProperties);

			A.CallTo(() => fakeActorRef.Start()).MustHaveHappened();
		}

		[Fact]
		public void Given_an_actor_Then_it_should_be_possible_to_create_a_child_actor_and_forward_messages_to_it()
		{
			var tuple = GetActorCreator();
			var actorCreator = tuple.Item1;

			CreateChildTestActor actor = null;
			var actorref = actorCreator.CreateActor(ActorCreationProperties.Create(() => { actor = new CreateChildTestActor(); return actor; }));

			actorref.Send("123", null);
			actor.ChildReceivedMessages.Should().ContainInOrder(new object[] { "123" });
		}



		private class CreateChildTestActor : Actor
		{
			public List<object> ChildReceivedMessages = new List<object>();
			public CreateChildTestActor()
			{
				var child = CreateActor(ActorCreationProperties.CreateAnonymous<object>(msg => ChildReceivedMessages.Add(msg)));
				ReceiveAny(m => child.Send(m, Self));
			}
		}

	}
	// ReSharper restore InconsistentNaming
}