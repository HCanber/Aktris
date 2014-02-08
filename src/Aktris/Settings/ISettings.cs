namespace Aktris.Settings
{
	public interface ISettings
	{
		bool DebugEventStream { get; }
		IStandardOutLoggerSettings StandardOutLoggerSettings { get; }
	}
}