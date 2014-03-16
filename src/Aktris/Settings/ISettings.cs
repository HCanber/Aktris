﻿using System;
using System.Collections;
using System.Collections.Generic;
using Aktris.Internals.Logging;
using Aktris.Logging;

namespace Aktris.Settings
{
	public interface ISettings
	{
		bool DebugEventStream { get; }
		IStandardOutLoggerSettings StandardOutLoggerSettings { get; }
		/// <summary>
		/// A collection of loggers. They must all inherit from <see cref="Actor"/>, except <see cref="StandardOutLogger"/>. 
		/// If this value is <c>null</c> then the <see cref="DefaultLogger"/> is used.
		/// </summary>
		IEnumerable<Type> Loggers { get; }
		bool DebugLifecycle { get; }
		bool DebugAutoHandle { get; }

		LogLevel LogLevel { get; }
	}
}