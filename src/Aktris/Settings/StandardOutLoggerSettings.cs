using Aktris.Internals.Logging;
using Aktris.Logging;

namespace Aktris.Settings
{
	public class StandardOutLoggerSettings : IStandardOutLoggerSettings
	{
		public StandardOutLoggerSettings()
		{
			LogLevel=LogLevel.Error;
			DateFormat = StandardOutLogger.DefaultDateFormat;
		}

		public LogLevel LogLevel { get; set; }
		public string DateFormat { get; set; }

		public StandardOutLoggerSettings DeepClone()
		{
			return (StandardOutLoggerSettings) MemberwiseClone();
		}
	}
}