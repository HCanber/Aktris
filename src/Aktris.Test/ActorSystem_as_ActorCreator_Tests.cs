using System;
using Aktris.Exceptions;
using Aktris.Internals;

namespace Aktris.Test
{
	// ReSharper disable InconsistentNaming
	public class ActorSystem_as_ActorCreator_Tests : ActorCreator_Tests_Helper
	{
		protected override Tuple<IActorCreator, ActorSystem> GetActorCreator(LocalActorRefFactory localActorRefFactory, IBootstrapper bootstrapper = null)
		{
			if(bootstrapper == null)
			{
				var testBootstrapper=new TestBootstrapper();
				if(localActorRefFactory != null)
				{
					testBootstrapper.LocalActorRefFactory = localActorRefFactory;
				}
				bootstrapper = testBootstrapper;
			}
			var system = new InternalActorSystem("default", bootstrapper);
			return new Tuple<IActorCreator, ActorSystem>(system, system);
		}
	}
	// ReSharper restore InconsistentNaming
}