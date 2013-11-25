using System;
using Aktris.Internals;

namespace Aktris.Test
{
// ReSharper disable InconsistentNaming
	public class ActorSystem_as_ActorCreator_Tests : ActorCreator_Tests_Helper
	{
		protected override Tuple<IActorCreator, ActorSystem> GetActorCreator(IBootstrapper bootstrapper=null)
		{
			var system = new InternalActorSystem("default", bootstrapper ?? new TestBootstrapper());
			return new Tuple<IActorCreator, ActorSystem>(system,system);
		}
	}
	// ReSharper restore InconsistentNaming
}