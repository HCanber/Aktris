using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace Aktris.Internals.Helpers
{
	/// <summary>
	/// Provides a instance of <see cref="Random"/> per thread, since <see cref="Random"/> is not thread safe.
	/// </summary>
	public static class RandomProvider
	{
		//Thanks Jon Skeet for this. http://csharpindepth.com/Articles/Chapter12/Random.aspx
		private static int _seed = Environment.TickCount;

		private static readonly ThreadLocal<Random> _RandomWrapper = new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref _seed)));

		public static Random GetThreadRandom()
		{
			return _RandomWrapper.Value;
		}

		public static uint GetNextUInt()
		{
			var bytes = new byte[4];
			GetThreadRandom().NextBytes(bytes);
			UInt32Converter asUInt = bytes;
			return asUInt;
		}
	}

	[StructLayout(LayoutKind.Explicit)]
	struct UInt32Converter
	{
		// ReSharper disable PrivateFieldCanBeConvertedToLocalVariable
		// ReSharper disable FieldCanBeMadeReadOnly.Local
		[FieldOffset(0)]
		private uint _value;
		[FieldOffset(0)]
		private byte _byte1;
		[FieldOffset(1)]
		private byte _byte2;
		[FieldOffset(2)]
		private byte _byte3;
		[FieldOffset(3)]
		private byte _byte4;

		public UInt32Converter(uint value)
		{
			_byte1 = _byte2 = _byte3 = _byte4 = 0;
			_value = value;
		}

		public static implicit operator UInt32(UInt32Converter value)
		{
			return value._value;
		}

		public static implicit operator UInt32Converter(uint value)
		{
			return new UInt32Converter(value);
		}

		public static implicit operator UInt32Converter(Stream stream)
		{
			var bytes = new byte[4];
			stream.Read(bytes, 0, 4);

			var value = new UInt32Converter(0)
			{
				_byte1 = bytes[0],
				_byte2 = bytes[1],
				_byte3 = bytes[2],
				_byte4 = bytes[3]
			};
			return value;
		}

		public static implicit operator UInt32Converter(byte[] bytes)
		{
			if(bytes == null || bytes.Length < 4) throw new ArgumentException("The array must contain at least 4 bytes.");

			var value = new UInt32Converter(0)
			{
				_byte1 = bytes[0],
				_byte2 = bytes[1],
				_byte3 = bytes[2],
				_byte4 = bytes[3]
			};
			return value;
		}

		public byte[] AsBytes()
		{
			return new byte[] { _byte1, _byte2, _byte3, _byte4 };
		}

		public void WriteTo(Stream stream)
		{
			if(stream is FileStream)
			{
				stream.WriteByte(_byte1);
				stream.WriteByte(_byte2);
				stream.WriteByte(_byte3);
				stream.WriteByte(_byte4);
			}
			else
			{
				stream.Write(AsBytes(), 0, 4);
			}
		}

		public uint Value { get { return _value; } }
		public byte Byte1 { get { return _byte1; } }
		public byte Byte2 { get { return _byte2; } }
		public byte Byte3 { get { return _byte3; } }
		public byte Byte4 { get { return _byte4; } }
	}
}