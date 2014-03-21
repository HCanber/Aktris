using System;
using System.Text;

namespace Aktris.Internals.Logging
{
	public static class ExceptionFormatter
	{
		public static string DebugFormat(Exception exception, string prefix = null, string exceptionsDelimiter = "\n\n")
		{
			var aggregateException = exception as AggregateException;
			if(aggregateException != null)
			{
				var exceptions = aggregateException.Flatten().InnerExceptions;
				var exceptionsCount = exceptions.Count;
				if(exceptionsCount == 0)
					return prefix;
				if(exceptionsCount == 1)
					return prefix + exceptions[0];
				var sb = new StringBuilder();
				sb.Append(prefix);
				sb.Append(exceptions[0]);
				for(var i = 1; i < exceptionsCount; i++)
				{
					sb.Append(exceptionsDelimiter);
					sb.Append(exceptions[i]);
				}
				return sb.ToString();
			}
			return prefix + exception;
		}
	}
}