///////////////////////////////////////////////////////
/// Filename: BytePayload.cs
/// Date: September 10, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Core;
using EppNet.Utilities;

using Microsoft.IO;

using System;
using System.Net;
using System.Text;

namespace EppNet.Data
{

    /// <summary>
    /// Essentially a wrapper around a <see cref="RecyclableMemoryStream"/>.
    /// All data is handled in little endian
    /// </summary>

    public class BytePayload : IDisposable
    {

        #region Static members
        public static int FloatPrecision = 4;

        public static readonly RecyclableMemoryStreamManager RecyclableStreamMgr = new RecyclableMemoryStreamManager();
        
        public static RecyclableMemoryStream ObtainStream()
        {
            return RecyclableStreamMgr.GetStream() as RecyclableMemoryStream;
        }

        public static RecyclableMemoryStream ObtainStream(byte[] buffer)
        {
            return RecyclableStreamMgr.GetStream(buffer) as RecyclableMemoryStream;
        }

        /// <summary>
        /// Converts a local float to a remote float (i.e. a float with the precision specified by <see cref="FloatPrecision"/>)
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>

        public static float FloatToNetFloat(float input)
        {
            int a = (int)(FastMath.Round(input, FloatPrecision) * FastMath.GetTenPow(FloatPrecision));
            return (float)a / (float)FastMath.GetTenPow(FloatPrecision);
        }
        
        #endregion

        public Encoding Encoder;

        protected internal RecyclableMemoryStream _stream;

        public byte[] PackedData { internal set; get; }

        public BytePayload()
        {
            this.Encoder = Encoding.UTF8;
            this.PackedData = null;
            this._stream = null;
        }

        public BytePayload(byte[] dataIn) : this()
        {
            _stream = ObtainStream(dataIn);
            _stream.Seek(0, System.IO.SeekOrigin.Begin);
        }

        public virtual byte[] Pack()
        {
            if (_stream == null)
                return null;

            if (PackedData == null)
            {
                this.PackedData = new byte[_stream.Length];
                _stream.WriteTo(PackedData);
            }

            return PackedData;
        }

        public virtual void Dispose()
        {
            _stream?.Dispose();
        }

        protected virtual void _EnsureReadyToWrite()
        {
            if (_stream == null)
                _stream = RecyclableStreamMgr.GetStream() as RecyclableMemoryStream;
        }

        public void WriteString(string input)
        {
            if (input.Length == 0 || input.Length > ushort.MaxValue)
                throw new ArgumentOutOfRangeException($"Datagram#WriteString(string) must be between 1 and {ushort.MaxValue}.");

            _EnsureReadyToWrite();

            WriteUInt16((ushort)input.Length);
            Span<byte> span = _stream.GetSpan(input.Length);
            ReadOnlySpan<char> chars = input.AsSpan();

            int written = Encoder.GetBytes(chars, span);
            _stream.Advance(written);
        }

        public string ReadString()
        {
            int length = ReadUInt16();
            Span<byte> buffer = stackalloc byte[length];
            int read = _stream.Read(buffer);

            string output = Encoder.GetString(buffer);

            return output;
        }

        public void WriteBool(bool input)
        {
            _EnsureReadyToWrite();
            WriteUInt8((byte) (input ? 1 : 0));
        }

        public bool ReadBool() => (ReadUInt8() == 1);

        public void WriteUInt8(byte input)
        {
            _EnsureReadyToWrite();

            Span<byte> buffer = stackalloc byte[1] { input };
            _stream.Write(buffer);
        }

        public byte ReadUInt8()
        {
            Span<byte> buffer = stackalloc byte[1];
            int read = _stream.Read(buffer);

            byte output = buffer[0];
            return output;
        }

        public void WriteByte(byte input) => WriteUInt8(input);
        public byte ReadByte() => ReadUInt8();

        public void WriteInt8(sbyte input)
        {
            _EnsureReadyToWrite();

            Span<byte> bytes = stackalloc byte[1] { Convert.ToByte(input) };
            _stream.Write(bytes);
        }

        public sbyte ReadInt8()
        {
            Span<byte> buffer = stackalloc byte[1];
            int read = _stream.Read(buffer);

            sbyte output = Convert.ToSByte(buffer[0]);
            return output;
        }

        public void WriteSByte(sbyte input) => WriteInt8(input);
        public sbyte ReadSByte() => ReadInt8();

        public void WriteInt16(short input)
        {
            _EnsureReadyToWrite();

            Span<byte> bytes = stackalloc byte[2];
            BitConverter.TryWriteBytes(bytes, IPAddress.HostToNetworkOrder(input));
            _stream.Write(bytes);
        }

        public short ReadInt16()
        {
            Span<byte> buffer = stackalloc byte[2];
            int read = _stream.Read(buffer);

            short output = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(buffer));
            return output;
        }

        public void WriteShort(short input) => WriteInt16(input);
        public short ReadShort() => ReadInt16();

        public void WriteUInt16(ushort input)
        {
            _EnsureReadyToWrite();

            Span<byte> bytes = stackalloc byte[2];
            BitConverter.TryWriteBytes(bytes, IPAddress.HostToNetworkOrder((short)input));
            _stream.Write(bytes);
        }

        public ushort ReadUInt16()
        {
            Span<byte> buffer = stackalloc byte[2];
            int read = _stream.Read(buffer);
            ushort output = (ushort) IPAddress.NetworkToHostOrder((short) BitConverter.ToInt16(buffer));

            return output;
        }

        public void WriteUShort(ushort input) => WriteUInt16(input);
        public ushort ReadUShort() => ReadUInt16();

        public void WriteUInt32(uint input)
        {
            _EnsureReadyToWrite();

            Span<byte> bytes = stackalloc byte[4];
            BitConverter.TryWriteBytes(bytes, (uint) IPAddress.HostToNetworkOrder((int)input));
            _stream.Write(bytes);
        }

        public uint ReadUInt32()
        {
            Span<byte> buffer = stackalloc byte[4];
            int read = _stream.Read(buffer);
            uint output = (uint)IPAddress.NetworkToHostOrder((int)BitConverter.ToInt32(buffer));

            return output;
        }

        public void WriteUInt(uint input) => WriteUInt32(input);
        public uint ReadUInt() => ReadUInt32();

        public void WriteInt32(int input)
        {
            _EnsureReadyToWrite();

            Span<byte> bytes = stackalloc byte[4];
            BitConverter.TryWriteBytes(bytes, IPAddress.HostToNetworkOrder(input));
            _stream.Write(bytes);
        }

        public int ReadInt32()
        {
            Span<byte> buffer = stackalloc byte[4];
            int read = _stream.Read(buffer);
            int output = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(buffer));

            return output;
        }

        public void WriteInt(int input) => WriteInt32(input);
        public int ReadInt() => ReadInt32();

        public void WriteUInt64(ulong input)
        {
            _EnsureReadyToWrite();

            Span<byte> bytes = stackalloc byte[8];
            BitConverter.TryWriteBytes(bytes, IPAddress.HostToNetworkOrder((long)input));
            _stream.Write(bytes);
        }

        public ulong ReadUInt64()
        {
            Span<byte> buffer = stackalloc byte[8];
            int read = _stream.Read(buffer);
            ulong output = (ulong) IPAddress.NetworkToHostOrder(BitConverter.ToInt64(buffer));

            return output;
        }

        public void WriteULong(ulong input) => WriteUInt64(input);
        public ulong ReadULong() => ReadUInt64();

        public void WriteInt64(long input)
        {
            _EnsureReadyToWrite();

            Span<byte> bytes = stackalloc byte[8];
            BitConverter.TryWriteBytes(bytes, IPAddress.HostToNetworkOrder(input));
            _stream.Write(bytes);
        }

        public long ReadInt64()
        {
            Span<byte> buffer = stackalloc byte[8];
            int read = _stream.Read(buffer);
            long output = IPAddress.NetworkToHostOrder(BitConverter.ToInt64(buffer));

            return output;
        }

        public void WriteLong(long input) => WriteInt64(input);
        public long ReadLong() => ReadInt64();

        public void WriteFloat(float input)
        {
            _EnsureReadyToWrite();

            int i32 = (int) (FastMath.Round(input, FloatPrecision) * FastMath.GetTenPow(FloatPrecision));
            WriteInt32(i32);
        }

        public void WriteSingle(float input) => WriteFloat(input);

        public float ReadFloat()
        {
            float i32 = (float) ReadInt32();
            return i32 / (float) FastMath.GetTenPow(FloatPrecision);
        }

        public float ReadSingle() => ReadFloat();

        public void WriteTypedTimestamp(Timestamp input)
        {
            _EnsureReadyToWrite();

            WriteUInt8((byte)input.Type);
            WriteLong(input.Value);
        }

        public Timestamp ReadTypedTimestamp()
        {
            byte type = ReadUInt8();
            long value = ReadLong();

            TimestampType tsType = (TimestampType)Enum.ToObject(typeof(TimestampType), type);
            return new Timestamp(tsType, false, value);
        }

        public void WriteTimestamp(Timestamp input)
        {
            _EnsureReadyToWrite();

            WriteLong(input.Value);
        }

        public Timestamp ReadTimestamp()
        {
            long value = ReadLong();
            return new Timestamp() { Value = value };
        }

        public double GetSizeKB()
        {
            float length = (_stream != null) ? _stream.Length : 0;
            return FastMath.Round(length * 0.001, 3);
        }

    }

}