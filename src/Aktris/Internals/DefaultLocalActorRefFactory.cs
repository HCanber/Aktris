namespace Aktris.Internals
{
	public class DefaultLocalActorRefFactory : LocalActorRefFactory
	{
		public override InternalActorRef CreateActor(ActorSystem system, ActorCreationProperties actorCreationProperties, InternalActorRef supervisor, ActorPath path)
		{
			var mailbox = actorCreationProperties.CreateMailbox() ?? system.CreateDefaultMailbox();
			return new LocalActorRef(system, actorCreationProperties, path, mailbox, supervisor);
		}
	}
}