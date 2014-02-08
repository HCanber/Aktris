using System;
using Aktris.Logging;

namespace Aktris.Internals.Logging
{
	public class ErrorLogEvent : LogEvent
	{
		private readonly Exception _cause;

		public ErrorLogEvent(string logSource, Type logClass, object message)
			: this(logSource, logClass, message, null)
		{
		}

		public ErrorLogEvent(string logSource, Type logClass, object message, Exception cause)
			: base(logSource, logClass, message)
		{
			_cause = cause;
		}

		public override LogLevel LogLevel { get { return LogLevel.Error; } }

		public Exception Cause { get { return _cause; } }
	}
}