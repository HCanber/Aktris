using System;
using System.Threading;
using Aktris.Logging;

namespace Aktris.Internals.Logging
{
	public abstract class LogEvent
	{
		private readonly string _logSource;
		private readonly Type _logClass;
		private readonly object _message;
		private readonly Thread _currentThread;
		private readonly DateTime _timestampUtc;

		public LogEvent(string logSource, Type logClass,object message)
		{
			_logSource = logSource;
			_logClass = logClass;
			_message = message;
			_currentThread = Thread.CurrentThread;
			_timestampUtc = DateTime.UtcNow;
		}

		public abstract LogLevel LogLevel { get; }

		public string LogSource { get { return _logSource; } }
		public Type LogClass { get { return _logClass; } }
		public Thread CurrentThread { get { return _currentThread; } }
		public DateTime TimestampUtc { get { return _timestampUtc; } }

		public object Message { get { return _message; } }
	}
}