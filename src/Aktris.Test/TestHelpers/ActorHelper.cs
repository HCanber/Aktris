using System;
using System.Collections;
using System.Collections.Immutable;
using System.Reflection;
using Aktris.Dispatching;
using Aktris.Internals;
using Aktris.Internals.Path;
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
		private static Actor CreateActorDirectly<T>(Func<T> createActor, out LocalActorRef localActorRef) where T : Actor
		{
			var testActorSystem = new TestActorSystem();
			localActorRef = new LocalActorRef(testActorSystem, A.Dummy<ActorInstantiator>(), new RootActorPath("fake"), testActorSystem.CreateDefaultMailbox(), A.Dummy<InternalActorRef>());
			LocalActorRefStack.PushActorRefToStack(localActorRef);
			var actor = createActor();
			LocalActorRefStack.PopActorAndMarkerFromStack();
			return actor;
		}

		/// <summary>
		/// This will create an actor. 
		/// <remarks>NOTE! Only use this in tests</remarks>
		/// </summary>
		public static T CreateActorDirectly<T>() where T : Actor, new()
		{
			LocalActorRef ignored;
			return (T)CreateActorDirectly(() => new T(), out ignored);
		}

		/// <summary>
		/// This will create an actor. 
		/// <remarks>NOTE! Only use this in tests</remarks>
		/// </summary>
		public static T CreateActorDirectly<T>(out LocalActorRef localActorRef) where T : Actor, new()
		{
			return (T)CreateActorDirectly(() => new T(), out localActorRef);
		}

		/// <summary>
		/// This will create an actor and initialize it.
		/// <remarks>NOTE! Only use this in tests</remarks>
		/// </summary>
		public static T CreateInitializedActorDirectly<T>() where T : Actor, new()
		{
			LocalActorRef actorRef;
			var actor= (T)CreateActorDirectly(() => new T(), out actorRef);
			actor.Init(actorRef);
			return actor;
		}
		/// <summary>
		/// This will create an actor and initialize it.
		/// <remarks>NOTE! Only use this in tests</remarks>
		/// </summary>
		public static T CreateInitializedActorDirectly<T>(Func<T> createActor) where T : Actor
		{
			LocalActorRef actorRef;
			var actor = (T)CreateActorDirectly<T>(createActor, out actorRef);
			actor.Init(actorRef);
			return actor;
		}

		public static ImmutableStack<LocalActorRef> GetActorRefStack()
		{
			return LocalActorRefStack.GetActorRefStack_ForTestingONLY() ?? ImmutableStack<LocalActorRef>.Empty;
		}
	}
}