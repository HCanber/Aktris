namespace Aktris.Internals.SystemMessages
{
	public class SuperviseActor : SystemMessage
	{
		private readonly InternalActorRef _actorToSupervise;

		public SuperviseActor(InternalActorRef actorToSupervise)
		{
			_actorToSupervise = actorToSupervise;
		}

		public InternalActorRef ActorToSupervise { get { return _actorToSupervise; } }
	}
}