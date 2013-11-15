using System;
using System.Runtime.Serialization;

namespace Aktris.Exceptions
{
	[Serializable]
	public class InvalidActorNameException : Exception
	{
		//
		// For guidelines regarding the creation of new exception types, see
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
		// and
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
		//


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