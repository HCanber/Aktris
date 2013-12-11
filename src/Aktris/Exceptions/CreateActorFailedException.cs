using System;
using System.Runtime.Serialization;
using Aktris.JetBrainsAnnotations;

namespace Aktris.Exceptions
{
	public class CreateActorFailedException : Exception
	{
		public ActorRef Actor { get; set; }

		public Exception Cause { get { return InnerException; } }

		public CreateActorFailedException(ActorRef actor, string message, Exception cause = null)
			: base(message, cause)
		{
			Actor = actor;
		}

		
		protected CreateActorFailedException(
			SerializationInfo info,
			StreamingContext context) : base(info, context)
		{
		}

	}
}