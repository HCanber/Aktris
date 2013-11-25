namespace Aktris
{
	public delegate bool MessageHandler(object message, SenderActorRef sender);
	public delegate bool MessageHandler<in TMessage>(TMessage message, SenderActorRef sender);
}