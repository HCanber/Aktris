using System;
using System.Collections;
using System.Collections.Immutable;
using System.Reflection;
using Aktris.Dispatching;
using Aktris.Internals;
using Aktris.JetBrainsAnnotations;
using FakeItEasy;

namespace Aktris.Test.TestHelpers
{
	public static class ActorHelper
	{
		/// <summary>
		/// This will decorate a factory method and make it possible to directly create an actor. 
		/// <remarks>NOTE! Only use this in tests</remarks>
		/// </summary>
		public static Func<Actor> CreateActorDirectly<T>(Func<T> createActor) where T : Actor
		{
			return () =>
			{
				LocalActorRefStack.PushActorRefToStack(A.Fake<LocalActorRef>());
				var actor = createActor();
				LocalActorRefStack.PopActorAndMarkerFromStack();
				return actor;
			};
		}

		/// <summary>
		/// This will decorate a factory method and make it possible to directly create an actor. 
		/// <remarks>NOTE! Only use this in tests</remarks>
		/// </summary>
		public static T CreateActorDirectly<T>() where T : Actor, new()
		{
			return (T) CreateActorDirectly<T>(() => new T())();
		}

		public static ImmutableStack<LocalActorRef> GetActorRefStack()
		{
			return LocalActorRefStack.GetActorRefStack_ForTestingONLY() ?? ImmutableStack<LocalActorRef>.Empty;
		}
	}
}