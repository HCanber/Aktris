using System;
using System.Runtime.Serialization;

namespace Aktris.Exceptions
{
	public class ActorKilledException: Exception
	{
		public ActorRef Actor { get; set; }

		public Exception Cause { get { return InnerException; } }

		public ActorKilledException(ActorRef actor, string message, Exception cause = null)
			: base(message, cause)
		{
			Actor = actor;
		}

		
		protected ActorKilledException(
			SerializationInfo info,
			StreamingContext context) : base(info, context)
		{
		}

	}
}