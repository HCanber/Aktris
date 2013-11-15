namespace Aktris
{
	public interface IActorCreator
	{
		ActorRef CreateActor(ActorCreationProperties actorCreationProperties, string name=null);
	}
}