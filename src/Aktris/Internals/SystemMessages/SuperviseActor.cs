namespace Aktris.Internals.SystemMessages
{
	public class SuperviseActor : SystemMessage
	{
		private readonly ActorRef _actorToSupervise;

		public SuperviseActor(ActorRef actorToSupervise)
		{
			_actorToSupervise = actorToSupervise;
		}

		public ActorRef ActorToSupervise { get { return _actorToSupervise; } }
	}
}