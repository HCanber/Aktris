namespace Aktris.Internals.Logging
{
	public class DefaultLogger : Actor
	{
		public DefaultLogger()
		{
			Receive<InitializeLogger>(_ => Sender.Reply(LoggerInitialized.Instance));
			Receive<LogEvent>(e => StandardOutLoggerHelper.Print(e));
		}
	}
}