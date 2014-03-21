using System.IO;
using System.Text;
using System.Threading;

namespace Aktris.Internals.Logging
{
	public class TemporaryFileLogger : Actor	//TODO: Replace this with something better
	{
		public TemporaryFileLogger()
		{
			FileStream fileStream = null;
			var newLine = Encoding.UTF8.GetBytes("\n");
			Receive<InitializeLogger>(_ =>
			{
				var exists = File.Exists("log.txt");
			
				if(exists)
				{
					File.Delete("log.txt");
				}
				fileStream = File.OpenWrite("log.txt");
				var preamble = Encoding.UTF8.GetPreamble();
				fileStream.Write(preamble, 0, preamble.Length);

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