namespace Aktris
{
	public abstract class ActorCreationProperties
	{
		public abstract Actor CreateNewActor();


		public static ActorCreationProperties Create<T>() where T : Actor, new()
		{
			//TODO: Create this thru the ioc container instead of Activator so that dependencies can be injected. Then remove:  T: new()
			return new DelegateActorCreationProperties(() => new T());
		}
	}
}