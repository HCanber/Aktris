using System;
using Aktris.JetBrainsAnnotations;

namespace Aktris.Exceptions
{
	public class NullMessageException : ArgumentNullException
	{
		public NullMessageException([InvokerParameterName]string paramName)
			: base(paramName, "Message is null")
		{
		}
	}
}