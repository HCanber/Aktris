namespace Aktris.Internals
{
	public abstract class LocalActorRefFactory
	{
		public abstract InternalActorRef CreateActor(ActorSystem system, ActorCreationProperties actorCreationProperties, InternalActorRef supervisor, ActorPath path);
	}
}