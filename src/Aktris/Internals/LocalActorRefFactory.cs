namespace Aktris.Internals
{
	public abstract class LocalActorRefFactory
	{
		public abstract InternalActorRef CreateActor(ActorSystem system, ActorCreationProperties actorCreationProperties, ActorPath path);
	}
}