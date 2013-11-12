using System.Text;

namespace Aktris.Internals
{
	public static class Base64Helper
	{
		private static readonly string _base64chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789+~";

		public static string Encode(long value)
		{
			var sb = new StringBuilder("$");
			var next = value;
			do
			{
				var index = (int)(next & 63);
				sb.Append(_base64chars[index]);
				next = next >> 6;
			} while(next != 0);
			return sb.ToString();
		}

	}
}