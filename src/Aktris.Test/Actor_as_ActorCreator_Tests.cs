using System;
using Aktris.Internals;

namespace Aktris.Test
{
// ReSharper disable once InconsistentNaming
	public class Actor_as_ActorCreator_Tests : ActorCreator_Tests_Helper
	{
		protected override Tuple<IActorCreator, ActorSystem> GetActorCreator(LocalActorRefFactory localActorRefFactory, IBootstrapper bootstrapper = null)
		{
			if(bootstrapper == null)
			{
				var testBootstrapper = new TestBootstrapper();
				if(localActorRefFactory != null)
					testBootstrapper.LocalActorRefFactory = localActorRefFactory;
				bootstrapper = testBootstrapper;
			}

			var system = new InternalActorSystem("default", bootstrapper);
			system.Start();
			Actor actor = null;
			system.CreateActor(ActorCreationProperties.Create(() =>
			{
				actor = new ParentActor();
				return actor;
			}),"Parent");
			return new Tuple<IActorCreator, ActorSystem>(actor, system);
		}

		private class ParentActor : Actor
		{
		}
	}
}