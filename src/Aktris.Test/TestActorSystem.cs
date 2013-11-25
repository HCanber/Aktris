namespace Aktris.Test
{
	public class TestActorSystem : ActorSystem
	{
		public TestActorSystem()
			: this(null)
		{
		}

		public TestActorSystem(IBootstrapper bootstrapper)
			: base("test", bootstrapper ?? new TestBootstrapper())
		{
		}
	}
}