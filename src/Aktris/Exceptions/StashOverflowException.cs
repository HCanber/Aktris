using System;
using System.Runtime.Serialization;

namespace Aktris.Exceptions
{
	[Serializable]
	public class StashOverflowException : Exception
	{
		// This constructor is needed for serialization.
		protected StashOverflowException(SerializationInfo info, StreamingContext context ) : base( info, context ) { }

		public StashOverflowException() { }

		public StashOverflowException(string message ) : base( message ) { }

		public StashOverflowException(string message, Exception inner) : base(message, inner) { }

	}
}