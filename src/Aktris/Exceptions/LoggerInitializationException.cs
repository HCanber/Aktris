using System;
using System.Runtime.Serialization;

namespace Aktris.Exceptions
{
	[Serializable]
	public class LoggerInitializationException : Exception
	{
		// This constructor is needed for serialization.
		protected LoggerInitializationException(SerializationInfo info, StreamingContext context ) : base( info, context ) { }

		public LoggerInitializationException() { }

		public LoggerInitializationException(string message ) : base( message ) { }

		public LoggerInitializationException(string message, Exception inner) : base(message, inner) { }

	}
}