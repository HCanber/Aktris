using System;
using Aktris.JetBrainsAnnotations;

namespace Aktris.Internals
{
	public class DeadLetterActorRef : ActorRef
	{
		private ActorPath _path;

		public DeadLetterActorRef([NotNull] ActorPath path)
		{
			if(path == null) throw new ArgumentNullException("path");
			_path = path;
		}

		public string Name { get { return _path.Name; } }
		public ActorPath Path { get { return _path; } }

		public void Send([NotNull] object message, ActorRef sender)
		{
			if(message == null) throw new ArgumentNullException("message");

			//For now just print to console
			var deadLetter = message is DeadLetterMessage ? (message) as DeadLetterMessage : new DeadLetterMessage(message, sender ?? this, this);
			var color = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(deadLetter);
			Console.ForegroundColor = color;
		}
	}
}