using System;
using Aktris.Internals.SystemMessages;
using Aktris.JetBrainsAnnotations;
using Aktris.Messages;

namespace Aktris.Internals
{
	public class DeadLetterActorRef : MinimalActorRef
	{
		private ActorPath _path;

		public DeadLetterActorRef([NotNull] ActorPath path) 
		{
			if(path == null) throw new ArgumentNullException("path");
			_path = path;
		}

		public override string Name { get { return _path.Name; } }
		public override ActorPath Path { get { return _path; } }
		public override uint InstanceId { get { return LocalActorRef.UndefinedInstanceId; } }

		public override void Send([NotNull] object message, ActorRef sender)
		{
			if(message == null) throw new ArgumentNullException("message");
			var deadLetterMessage = message as DeadLetterMessage;
			if(deadLetterMessage == null)
			{
				deadLetterMessage=new DeadLetterMessage(message,sender,this);
			}
			if(SpecialHandle(deadLetterMessage.Message, deadLetterMessage.Sender))
				return;
			//For now just print to console
			var deadLetter = message is DeadLetterMessage ? (message) as DeadLetterMessage : new DeadLetterMessage(message, sender ?? this, this);
			var color = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(deadLetter);
			Console.ForegroundColor = color;
		}

		private bool SpecialHandle(object message, ActorRef sender)
		{
			var watchMessage = message as WatchActor;
			if(watchMessage != null)
			{
				if(watchMessage.Watcher != this && watchMessage.Watchee != this)
				{
					watchMessage.Watcher.SendSystemMessage(new ActorTerminated(watchMessage.Watchee),watchMessage.Watchee);
					return true;
				}
			}
			return false;
		}

		public override ActorRef CreateActor(ActorCreationProperties actorCreationProperties, string name = null)
		{
			throw new InvalidOperationException("The DeadLetter Actor may not have children.");
		}

	}
}