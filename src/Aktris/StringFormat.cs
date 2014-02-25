using System;
using System.Text;
using Aktris.JetBrainsAnnotations;

namespace Aktris
{
	public static class StringFormat
	{
		[NotNull, StringFormatMethod("message")]
		public static string SafeFormat(string message, params object[] args)
		{
			if(message == null) return "";
			if(args == null) return message;
			var length = args.Length;
			if(length == 0) return message;
			object[] values;
			try
			{
				values = ConvertValues(args, length);
			}
			catch(Exception e)
			{
				return "Could not format string. Failed while calling ToString on values. Exception: " + e + ", Message: \"" + message + "\"";
			}
			try
			{
				return string.Format(null, message, values);
			}
			catch(FormatException exception)
			{
				return "Could not format string. Message: \"" + message + "\". Arguments: " + string.Join(", ", values);
			}
		}

		[StringFormatMethod("message")]
		public static void AppendSafeFormat(this StringBuilder sb, string message, params object[] args)
		{
			if(message == null) return;
			if(args == null)
			{
				sb.Append(message);
				return;
			}
			var length = args.Length;
			if(length == 0)
			{
				sb.Append(message);
				return;
			}
			object[] values;
			try
			{
				values = ConvertValues(args, length);
			}
			catch(Exception e)
			{
				sb.Append("Could not format string. Failed while calling ToString on values. Exception: ").Append(e).Append(", Message: \"").Append(message).Append("\"");
				return;
			}
			try
			{
				sb.AppendFormat(null, message, values);
			}
			catch(FormatException e)
			{
				sb.Append("Could not format string. Message: \"").Append(message).Append("\". Arguments: ").Append(string.Join(", ", values));
			}
		}

		private static object[] ConvertValues(object[] args, int length)
		{
			var values = new object[length];
			for(int i = 0; i < length; i++)
			{
				var arg = args[i];
				values[i] = arg == null ? "<null>" : arg.ToString();
			}
			return values;
		}
	}

}