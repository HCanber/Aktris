namespace Aktris.Internals
{
	public abstract class LocalActorRefFactory
	{
		public abstract ILocalActorRef CreateActor(ActorCreationProperties actorCreationProperties, string name);
	}
}