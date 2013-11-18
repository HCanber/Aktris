using System;
using System.Runtime.Serialization;

namespace Aktris.Exceptions
{
	[Serializable]
	public class InvalidActorNameException : Exception
	{
		public InvalidActorNameException(string message) : base(message)
		{
		}

		public InvalidActorNameException(string message, Exception inner) : base(message, inner)
		{
		}

		protected InvalidActorNameException(
			SerializationInfo info,
			StreamingContext context) : base(info, context)
		{
		}

	}
}