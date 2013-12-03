namespace Aktris
{
	// ReSharper disable once InconsistentNaming
	public interface ActorRef	//TODO : IComparable<ActorRef>, IEquatable<ActorRef>
	{
		string Name { get; }
		ActorPath Path { get; }

		void Send(object message, ActorRef sender);
	}
}