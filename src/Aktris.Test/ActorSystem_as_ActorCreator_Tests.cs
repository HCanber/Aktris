using Aktris.Internals;

namespace Aktris.Test
{
// ReSharper disable InconsistentNaming
	public class ActorSystem_as_ActorCreator_Tests : ActorCreator_Tests_Helper
	{
		protected override IActorCreator GetActorCreator(IBootstrapper bootstrapper=null)
		{
			return new InternalActorSystem("default", bootstrapper ?? DefaultActorSystemFactory.Instance);
		}
	}
	// ReSharper restore InconsistentNaming
}