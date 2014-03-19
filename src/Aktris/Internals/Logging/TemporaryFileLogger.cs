using System.IO;
using System.Text;

namespace Aktris.Internals.Logging
{
	public class TemporaryFileLogger : Actor
	{
		public TemporaryFileLogger()
		{
			FileStream fileStream = null;
			var newLine = Encoding.UTF8.GetBytes("\n");
			Receive<InitializeLogger>(_ =>
			{
				var exists = File.Exists("log.txt");
				fileStream = File.OpenWrite("log.txt");
				if(!exists)
				{
					var preamble = Encoding.UTF8.GetPreamble();
					fileStream.Write(preamble,0,preamble.Length);
				}
				Sender.Reply(LoggerInitialized.Instance);				
			});
			Receive<LogEvent>(e =>
			{
				var line = LineLogFormatter.Format(e,LineLogFormatter.DefaultDateFormat);
				var bytes = Encoding.UTF8.GetBytes(line);
				fileStream.Write(bytes, 0, bytes.Length);
				fileStream.Write(newLine, 0, newLine.Length);
				fileStream.Flush();
			});
		}
	}
}