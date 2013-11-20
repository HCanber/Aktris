namespace Aktris.Internals
{
	public abstract class LocalActorRefFactory
	{
		public abstract ILocalActorRef CreateActor(ActorSystem system, ActorCreationProperties actorCreationProperties, string name);
	}
}