using System;
using System.Runtime.Serialization;
using Aktris.JetBrainsAnnotations;

namespace Aktris.Exceptions
{
	public class NullMessageException : ArgumentNullException
	{
		public NullMessageException([InvokerParameterName]string paramName)
			: base(paramName, "Message is null")
		{
		}

		protected NullMessageException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}