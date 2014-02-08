using System;
using System.Globalization;
using Aktris.Internals.Path;
using Aktris.JetBrainsAnnotations;

namespace Aktris.Internals.Logging
{
	public class StandardOutLogger : MinimalActorRef
	{
		private static readonly object _ConsoleLock = new object();
		private readonly ActorPath _path;
		private readonly string _dateFormat;
		private ActorSystem _system;
		public const string DefaultDateFormat = "HH:mm:ss.FFF";
		private const string _ErrorFormatException = "ERROR {0} [{1}] {2}: {3}{4}";// 0=Time, 1=Thread, 2=LogSource, 3=Message, Exception
		private const string _ErrorFormat = "ERROR {0} [{1}] {2}: {3}"; // 0=Time, 1=Thread, 2=LogSource, 3=Message
		private const string _WarningFormat = "WARN  {0} [{1}] {2}: {3}";
		private const string _InfoFormat = "INFO  {0} [{1}] {2}: {3}";
		private const string _DebugFormat = "DEBUG {0} [{1}] {2}: {3}";



		public StandardOutLogger(ActorPath path, ActorSystem system, string dateFormat = DefaultDateFormat)
		{
			_path = path;
			_system = system;
			_dateFormat = dateFormat ?? DefaultDateFormat;
		}

		public override ActorSystem System { get { return _system; } }

		public override string Name { get { return _path.Name; } }

		public override ActorPath Path { get { return _path; } }

		public override uint InstanceId { get { return _path.InstanceId; } }

		public override void Send([NotNull] object message, ActorRef sender)
		{
			Print(message);
		}

		private void Print(object message)
		{
			if(message == null) throw new ArgumentNullException("message");
			// ReSharper disable ConvertClosureToMethodGroup
			var ignored = PatternMatcher.Match<ErrorLogEvent>(message, m => PrintError(m))
										|| PatternMatcher.Match<WarningLogEvent>(message, m => PrintWarning(m))
										|| PatternMatcher.Match<InfoLogEvent>(message, m => PrintInfo(m))
										|| PatternMatcher.Match<DebugLogEvent>(message, m => PrintDebug(m))
										|| PatternMatcher.MatchAll(() => PrintWarning(new WarningLogEvent(GetType().Name, GetType(), "Received unexpected message of type " + message.GetType() + ": " + message)));
			// ReSharper restore ConvertClosureToMethodGroup
		}

		public void PrintError(ErrorLogEvent e)
		{
			var dateFormat = _dateFormat;
			PrintError(e, dateFormat);
		}

		public static void PrintError(ErrorLogEvent e, string dateFormat)
		{
			if(e.Cause == null)
			WriteLine(e, _ErrorFormat, dateFormat, ConsoleColor.Red);
			else
				WriteLine(e, _ErrorFormatException, dateFormat, ConsoleColor.Red, e.Cause.StackTrace);
		
		}

		public void PrintWarning(WarningLogEvent e)
		{
			WriteLine(e, _WarningFormat, _dateFormat,ConsoleColor.Magenta);
		}

		public void PrintInfo(InfoLogEvent e)
		{
			WriteLine(e, _InfoFormat, _dateFormat, ConsoleColor.White);
		}

		public void PrintDebug(DebugLogEvent e)
		{
			WriteLine(e, _DebugFormat, _dateFormat, ConsoleColor.Gray);
		}

		private static void WriteLine(LogEvent e, string format, string dateFormat, ConsoleColor foregroundColor, object extra = null)
		{
			var msg = StringFormat.SafeFormat(format, e.TimestampUtc.ToLocalTime().ToString(dateFormat), e.CurrentThread.Name ?? e.CurrentThread.ManagedThreadId.ToString(CultureInfo.InvariantCulture), e.LogSource, e.Message, extra);
			WriteLine(msg, foregroundColor);
		}

		private static void WriteLine(string msg, ConsoleColor foregroundColor)
		{
			lock(_ConsoleLock)
			{
				var color = Console.ForegroundColor;
				Console.ForegroundColor = foregroundColor;
				Console.WriteLine(msg);
				Console.ForegroundColor = color;
			}
		}

		public override string ToString()
		{
			return "StandardOutLogger";
		}
	}
}