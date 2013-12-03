namespace Aktris.Internals
{
	public class DefaultLocalActorRefFactory : LocalActorRefFactory
	{
		public override InternalActorRef CreateActor(ActorSystem system, ActorCreationProperties actorCreationProperties, ActorPath path)
		{
			var mailbox = actorCreationProperties.CreateMailbox() ?? system.CreateDefaultMailbox();
			return new LocalActorRef(system, actorCreationProperties, path,mailbox);
		}
	}
}