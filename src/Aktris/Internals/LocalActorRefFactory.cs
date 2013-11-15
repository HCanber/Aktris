namespace Aktris.Internals
{
	public abstract class LocalActorRefFactory
	{
		public abstract ActorRef CreateActor(ActorFactory actorFactory, string name);
	}
}