using System;
using Aktris.Logging;
using FluentAssertions;
using Xunit;
using Xunit.Extensions;

namespace Aktris.Test.Logging
{
	public class LogLevelTests
	{
		[Theory,
		InlineData(LogLevel.Off, 0),
		InlineData(LogLevel.Error, 1),
		InlineData(LogLevel.Warning, 2),
		InlineData(LogLevel.Info, 3),
		InlineData(LogLevel.Debug, 4),
		]
		public void Level_MustBe_Value(LogLevel logLevel, int value)
		{
			((int)logLevel).Should().Be(value);
		} 
	}
}