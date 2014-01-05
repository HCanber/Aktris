namespace Aktris.Internals.SystemMessages
{
	public class ActorTerminated : SystemMessage
	{
		private readonly InternalActorRef _terminatedActor;

		public ActorTerminated(InternalActorRef terminatedActor)
		{
			_terminatedActor = terminatedActor;
		}

		public InternalActorRef TerminatedActor { get { return _terminatedActor; } }
	}
}