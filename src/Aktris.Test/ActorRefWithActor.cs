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

		public string Name
		{
			get { return _actorRef.Name; } }

		public T Actor { get { return _actor; } }

		public void Send(object message, ActorRef sender)
		{
			_actorRef.Send(message, sender);
		}
	}
}