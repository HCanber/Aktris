using System;
using Aktris.Logging;

namespace Aktris.Internals.Logging
{
	public class InfoLogEvent : LogEvent
	{
		public InfoLogEvent(string logSource, Type logClass, object message)
			: base(logSource, logClass, message)
		{
		}

		public override LogLevel LogLevel { get { return LogLevel.Info; } }
	}
}