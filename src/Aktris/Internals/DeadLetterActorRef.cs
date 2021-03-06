﻿using System;
using Aktris.Internals.SystemMessages;
using Aktris.JetBrainsAnnotations;
using Aktris.Messages;

namespace Aktris.Internals
{
	public class DeadLetterActorRef : EmptyLocalActorRef
	{
		public DeadLetterActorRef([NotNull] ActorPath path, [NotNull] ActorSystem actorSystem)
			: base(path, actorSystem)
		{
		}

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



	}
}