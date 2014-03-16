using System;
using System.Collections.Generic;
using Aktris.Internals.Logging;
using Aktris.Logging;

namespace Aktris.Settings
{
	public class Settings : ISettings
	{
		private List<Type> _loggers;

		public Settings()
		{
			StandardOutLoggerSettings = new StandardOutLoggerSettings();
			LogLevel = LogLevel.Error;
		}

		public bool DebugEventStream { get; set; }
		public StandardOutLoggerSettings StandardOutLoggerSettings { get; set; }
		IStandardOutLoggerSettings ISettings.StandardOutLoggerSettings { get { return StandardOutLoggerSettings; } }
		public IEnumerable<Type> Loggers { get { return _loggers; } }
		public LogLevel LogLevel { get; set; }
		public bool DebugAutoHandle { get; set; }
		public bool DebugLifecycle { get; set; }
		public bool DebugMessages { get; set; }
		public bool DebugSystemMessages { get; set; }

		public bool EnableStandardOutLogger
		{
			get { return _loggers == null || _loggers.Contains(typeof(StandardOutLogger)); }
			set
			{
				if(value)
				{
					if(_loggers != null) _loggers.Add(typeof(StandardOutLogger));
					//else _loggers==null, meaning it's enable implicitly
				}
				else
				{
					if(_loggers != null) _loggers.Remove(typeof(StandardOutLogger));
					else { _loggers = new List<Type>(); }
				}
			}
		}


		public void AddLogger(Type type)
		{
			if(!typeof(Actor).IsAssignableFrom(type)) throw new ArgumentException(string.Format("The specified type {0} must implement {1}", type.FullName, typeof(Actor).Name));
			if(_loggers == null)
			{
				_loggers = new List<Type>();
			}
			_loggers.Add(type);
		}

		public Settings DeepClone()
		{
			var clone = (Settings)MemberwiseClone();
			clone.StandardOutLoggerSettings = StandardOutLoggerSettings.DeepClone();
			return clone;
		}
	}
}