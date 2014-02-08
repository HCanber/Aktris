using Aktris.Logging;

namespace Aktris.Settings
{
	public interface IStandardOutLoggerSettings
	{
		LogLevel LogLevel { get; }
		string DateFormat { get; }
	}
}