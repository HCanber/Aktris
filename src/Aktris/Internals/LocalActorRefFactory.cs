namespace Aktris.Internals
{
	public abstract class LocalActorRefFactory
	{
		public abstract ILocalActorRef CreateActor(ActorFactory actorFactory, string name);
	}
}