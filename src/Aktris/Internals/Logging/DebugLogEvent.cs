using System;
using Aktris.Logging;

namespace Aktris.Internals.Logging
{
	public class DebugLogEvent : LogEvent
	{
		public DebugLogEvent(string logSource, Type logClass, object message)
			: base(logSource, logClass, message)
		{
		}

		public override LogLevel LogLevel { get { return LogLevel.Debug; } }
	}
}