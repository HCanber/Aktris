namespace Aktris.Internals
{
	public class DefaultLocalActorRefFactory : LocalActorRefFactory
	{
		public override ILocalActorRef CreateActor(ActorSystem system, ActorCreationProperties actorCreationProperties, string name)
		{
			var mailbox = actorCreationProperties.CreateMailbox();
			return new LocalActorRef(system, actorCreationProperties, name,mailbox);
		}
	}
}