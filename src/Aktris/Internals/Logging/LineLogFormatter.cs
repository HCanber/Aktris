using System.Globalization;

namespace Aktris.Internals.Logging
{
	public static class LineLogFormatter
	{
		public const string DefaultDateFormat = "HH:mm:ss.FFF";
		private const string _ErrorFormatException = "ERROR {0} [{1}] {2}: {3}{4}";// 0=Time, 1=Thread, 2=LogSource, 3=Message, Exception
		private const string _ErrorFormat = "ERROR {0} [{1}] {2}: {3}"; // 0=Time, 1=Thread, 2=LogSource, 3=Message
		private const string _WarningFormat = "WARN  {0} [{1}] {2}: {3}";
		private const string _InfoFormat = "INFO  {0} [{1}] {2}: {3}";
		private const string _DebugFormat = "DEBUG {0} [{1}] {2}: {3}";

		public static string FormatError(ErrorLogEvent e, string dateFormat)
		{
			return e.Cause == null
				? FormatMessage(e, _ErrorFormat, dateFormat)
				: FormatMessage(e, _ErrorFormatException, dateFormat, e.Cause.StackTrace);
		}

		public static string FormatWarning(WarningLogEvent e, string dateFormat)
		{
			return FormatMessage(e, _WarningFormat, dateFormat);
		}

		public static string FormatInfo(InfoLogEvent e, string dateFormat)
		{
			return FormatMessage(e, _InfoFormat, dateFormat);
		}

		public static string FormatDebug(DebugLogEvent e, string dateFormat)
		{
			return FormatMessage(e, _DebugFormat, dateFormat);
		}

		public static string FormatMessage(LogEvent e, string format, string dateFormat, object extra = null)
		{
			return StringFormat.SafeFormat(format, e.TimestampUtc.ToLocalTime().ToString((string)dateFormat), e.CurrentThread.Name ?? e.CurrentThread.ManagedThreadId.ToString(CultureInfo.InvariantCulture), e.LogSource, e.Message, extra);
		}
	}
}