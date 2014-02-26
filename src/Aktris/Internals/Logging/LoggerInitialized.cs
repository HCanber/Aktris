namespace Aktris.Internals.Logging
{
	public class LoggerInitialized
	{
		private static readonly LoggerInitialized _instance = new LoggerInitialized();

		private LoggerInitialized() {/* Intentionally left blank */}

		public static LoggerInitialized Instance { get { return _instance; } }
	}
}