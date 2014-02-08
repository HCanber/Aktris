using System;
using Aktris.Logging;

namespace Aktris.Internals.Logging
{
	public class WarningLogEvent : LogEvent
	{
		public WarningLogEvent(string logSource, Type logClass, object message)
			: base(logSource, logClass, message)
		{
		}

		public override LogLevel LogLevel { get { return LogLevel.Warning; } }
	}
}