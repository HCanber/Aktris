namespace Aktris.Internals
{
	public class DefaultLocalActorRefFactory : LocalActorRefFactory
	{
		public override InternalActorRef CreateActor(ActorSystem system, ActorCreationProperties actorCreationProperties, string name)
		{
			var mailbox = actorCreationProperties.CreateMailbox() ?? system.CreateDefaultMailbox();
			return new LocalActorRef(system, actorCreationProperties, name,mailbox);
		}
	}
}