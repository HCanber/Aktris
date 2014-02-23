namespace Aktris.Test
{
	public class ActorRefWithActor<T> : ActorRef where T:Actor
	{
		private readonly ActorRef _actorRef;
		private readonly T _actor;

		public ActorRefWithActor(ActorRef actorRef, T actor)
		{
			_actorRef = actorRef;
			_actor = actor;
		}

		public ActorRef InnerActorRef { get { return _actorRef; } }

		public string Name { get { return _actorRef.Name; } }

		public ActorPath Path { get { return _actorRef.Path; } }

		public T Actor { get { return _actor; } }

		public void Send(object message, ActorRef sender)
		{
			_actorRef.Send(message, sender);
		}
	}
}