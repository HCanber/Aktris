namespace Aktris.Internals
{
	public class DefaultLocalActorRefFactory : LocalActorRefFactory
	{
		public override ILocalActorRef CreateActor(ActorCreationProperties actorCreationProperties, string name)
		{
			var mailbox = actorCreationProperties.CreateMailbox();
			return new LocalActorRef(actorCreationProperties, name,mailbox);
		}
	}
}