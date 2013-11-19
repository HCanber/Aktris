using System;
using System.Runtime.Serialization;
using Aktris.JetBrainsAnnotations;

namespace Aktris.Exceptions
{
	public class ActorInitializationException : Exception
	{
		public ActorRef Actor { get; set; }

		public Exception Cause { get { return InnerException; } }

		public ActorInitializationException(ActorRef actor, string message, Exception cause = null)
			: base(message, cause)
		{
			Actor = actor;
		}

		
		protected ActorInitializationException(
			SerializationInfo info,
			StreamingContext context) : base(info, context)
		{
		}

	}
}