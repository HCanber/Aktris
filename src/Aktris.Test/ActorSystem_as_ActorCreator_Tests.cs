namespace Aktris.Test
{
// ReSharper disable InconsistentNaming
	public class ActorSystem_as_ActorCreator_Tests : ActorCreator_Tests_Helper
	{
		protected override IActorCreator GetActorCreator()
		{
			return ActorSystem.Create();
		}
	}
	// ReSharper restore InconsistentNaming
}