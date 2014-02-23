using System;

namespace Aktris.Exceptions
{
	public class AskTimeoutException : TimeoutException
	{
		public AskTimeoutException(string message)
			: base(message)
		{
		}

		public AskTimeoutException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}