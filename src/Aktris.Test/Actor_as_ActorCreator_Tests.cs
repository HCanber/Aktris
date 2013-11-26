using System;
using Aktris.Internals;

namespace Aktris.Test
{
// ReSharper disable once InconsistentNaming
	public class Actor_as_ActorCreator_Tests : ActorCreator_Tests_Helper
	{
		protected override Tuple<IActorCreator, ActorSystem> GetActorCreator(LocalActorRefFactory localActorRefFactory, IBootstrapper bootstrapper = null)
		{
			bootstrapper = bootstrapper ?? new TestBootstrapper();
			localActorRefFactory = localActorRefFactory ?? bootstrapper.LocalActorRefFactory;
			var system = new InternalActorSystem("default", bootstrapper);
			Actor actor = null;
			system.CreateActor(ActorCreationProperties.Create(() =>
			{
				actor = new ParentActor(system, localActorRefFactory);
				return actor;
			}),"Parent");
			return new Tuple<IActorCreator, ActorSystem>(actor, system);
		}

		private class ParentActor : Actor
		{
			public ParentActor(ActorSystem system, LocalActorRefFactory localActorRefFactory):base(null, system,localActorRefFactory)
			{
				
			}
		}
	}
}