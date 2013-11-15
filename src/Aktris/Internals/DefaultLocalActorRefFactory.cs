namespace Aktris.Internals
{
	public class DefaultLocalActorRefFactory : LocalActorRefFactory
	{
		public override ActorRef CreateActor(ActorFactory actorFactory, string name)
		{
			return new LocalActorRef(actorFactory, name);
		}
	}
}