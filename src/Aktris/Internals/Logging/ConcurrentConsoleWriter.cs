using System;

namespace Aktris.Internals.Logging
{
	public static class ConcurrentConsoleWriter
	{
		private static readonly object _ConsoleLock = new object();

		public static void WriteLine(string message, ConsoleColor? foregroundColor = null)
		{
			if(foregroundColor.HasValue)
			{
				lock(_ConsoleLock)
				{
					var color = Console.ForegroundColor;
					Console.ForegroundColor = foregroundColor.Value;
					Console.WriteLine(message);
					Console.ForegroundColor = color;
				}
			}
			else
			{
				lock(_ConsoleLock)
				{
					Console.WriteLine(message);
				}
			}
		}
	}
}