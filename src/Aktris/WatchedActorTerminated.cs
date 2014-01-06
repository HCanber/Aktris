namespace Aktris
{
	public class WatchedActorTerminated
	{
		private readonly ActorRef _terminatedActor;

		public WatchedActorTerminated(ActorRef terminatedActor)
		{
			_terminatedActor = terminatedActor;
		}

		public ActorRef TerminatedActor { get { return _terminatedActor; } }
	}
}