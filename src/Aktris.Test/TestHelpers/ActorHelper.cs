﻿using System;
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
		public static Func<Actor> CreateActorDirectly<T>(Func<T> createActor) where T : Actor
		{
			return () =>
			{
				var testActorSystem = new TestActorSystem();
				LocalActorRefStack.PushActorRefToStack(new LocalActorRef(testActorSystem, A.Dummy<ActorInstantiator>(), new RootActorPath("fake"), testActorSystem.CreateDefaultMailbox(), A.Dummy<InternalActorRef>()));
				var actor = createActor();
				LocalActorRefStack.PopActorAndMarkerFromStack();
				return actor;
			};
		}

		/// <summary>
		/// This will create an actor. 
		/// <remarks>NOTE! Only use this in tests</remarks>
		/// </summary>
		public static T CreateActorDirectly<T>() where T : Actor, new()
		{
			return (T)CreateActorDirectly<T>(() => new T())();
		}

		/// <summary>
		/// This will create an actor and initialize it.
		/// <remarks>NOTE! Only use this in tests</remarks>
		/// </summary>
		public static T CreateInitializedActorDirectly<T>() where T : Actor, new()
		{
			var actor = CreateActorDirectly<T>();
			actor.Init();
			return actor;
		}
		/// <summary>
		/// This will create an actor and initialize it.
		/// <remarks>NOTE! Only use this in tests</remarks>
		/// </summary>
		public static T CreateInitializedActorDirectly<T>(Func<T> createActor) where T : Actor
		{
			var actor = (T)CreateActorDirectly<T>(createActor)();
			actor.Init();
			return actor;
		}

		public static ImmutableStack<LocalActorRef> GetActorRefStack()
		{
			return LocalActorRefStack.GetActorRefStack_ForTestingONLY() ?? ImmutableStack<LocalActorRef>.Empty;
		}
	}
}