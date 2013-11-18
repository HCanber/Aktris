namespace Aktris
{
// ReSharper disable once InconsistentNaming
	public abstract class Actor
	{
		protected internal SenderActorRef Sender { get; internal set; }

		internal protected virtual void Receive(object message)
		{
			throw new System.NotImplementedException();
		}
	}
}