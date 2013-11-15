namespace Aktris.Internals
{
	public class DefaultLocalActorRefFactory : LocalActorRefFactory
	{
		public override ILocalActorRef CreateActor(ActorCreationProperties actorCreationProperties, string name)
		{
			return new LocalActorRef(actorCreationProperties, name);
		}
	}
}