namespace Aktris
{
	public interface IActorCreator
	{
		ActorRef CreateActor(ActorFactory actorFactory, string name=null);
	}
}