using Aktris.JetBrainsAnnotations;

namespace Aktris.Internals.Logging
{
	public class InitializeLogger
	{
		private readonly LoggingEventBus _loggingBus;

		public InitializeLogger(LoggingEventBus loggingBus)
		{
			_loggingBus = loggingBus;
		}

		[NotNull]
		public LoggingEventBus LoggingBus { get { return _loggingBus; } }
	}
}