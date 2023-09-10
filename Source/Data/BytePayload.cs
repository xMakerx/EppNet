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
        public static int FloatPrecision
        {
            set
            {
                if (_float_digits != value)
                {
                    _precision_number = -1;
                    GetPrecisionNumber();
                }
            }
            get => _float_digits;
        }

        protected static int _float_digits = 4;

        protected static int _precision_number = -1;

        protected static float _precision_reciprocal = 0f;

        public static int GetPrecisionNumber()
        {
            if (_precision_number == -1)
            {
                _precision_number = (int)Math.Pow(10.0d, (double)FloatPrecision);
                _precision_reciprocal = (float)Math.ReciprocalSqrtEstimate(GetPrecisionNumber() * GetPrecisionNumber());
            }

            return _precision_number;
        }

        public static float GetPrecisionReciprocal() => _precision_reciprocal;

        public static readonly RecyclableMemoryStreamManager RecyclableStreamMgr = new RecyclableMemoryStreamManager();
        
        public static RecyclableMemoryStream ObtainStream()
        {
            return RecyclableStreamMgr.GetStream() as RecyclableMemoryStream;
        }

        public static RecyclableMemoryStream ObtainStream(byte[] buffer)
        {
            return RecyclableStreamMgr.GetStream(buffer) as RecyclableMemoryStream;
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
            byte[] buffer = new byte[length];

            int read = _stream.Read(buffer, 0, buffer.Length);

            string output = Encoder.GetString(buffer, 0, read);
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

            byte[] bytes = new byte[] { input };
            _stream.Write(bytes, 0, bytes.Length);
        }

        public byte ReadUInt8()
        {
            byte[] buffer = new byte[1];
            int read = _stream.Read(buffer, 0, 1);

            byte output = buffer[0];
            return output;
        }

        public void WriteByte(byte input) => WriteUInt8(input);
        public byte ReadByte() => ReadUInt8();

        public void WriteInt8(sbyte input)
        {
            _EnsureReadyToWrite();

            byte[] bytes = new byte[] { Convert.ToByte(input) };
            _stream.Write(bytes, 0, bytes.Length);
        }

        public sbyte ReadInt8()
        {
            byte[] buffer = new byte[1];
            int read = _stream.Read(buffer, 0, buffer.Length);

            sbyte output = Convert.ToSByte(buffer[0]);
            return output;
        }

        public void WriteSByte(sbyte input) => WriteInt8(input);
        public sbyte ReadSByte() => ReadInt8();

        public void WriteInt16(short input)
        {
            _EnsureReadyToWrite();

            byte[] bytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(input));
            _stream.Write(bytes, 0, bytes.Length);
        }

        public short ReadInt16()
        {
            byte[] buffer = new byte[2];
            int read = _stream.Read(buffer, 0, buffer.Length);

            short output = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(buffer));
            return output;
        }

        public void WriteShort(short input) => WriteInt16(input);
        public short ReadShort() => ReadInt16();

        public void WriteUInt16(ushort input)
        {
            _EnsureReadyToWrite();

            byte[] bytes = BitConverter.GetBytes((ushort)IPAddress.HostToNetworkOrder((short) input));
            _stream.Write(bytes, 0, bytes.Length);
        }

        public ushort ReadUInt16()
        {
            byte[] buffer = new byte[2];
            int read = _stream.Read(buffer, 0, buffer.Length);
            ushort output = (ushort) IPAddress.NetworkToHostOrder((short)BitConverter.ToInt16(buffer));

            return output;
        }

        public void WriteUShort(ushort input) => WriteUInt16(input);
        public ushort ReadUShort() => ReadUInt16();

        public void WriteUInt32(uint input)
        {
            _EnsureReadyToWrite();

            byte[] bytes = BitConverter.GetBytes((uint) IPAddress.HostToNetworkOrder((int) input));
            _stream.Write(bytes, 0, bytes.Length);
        }

        public uint ReadUInt32()
        {
            byte[] buffer = new byte[4];
            int read = _stream.Read(buffer, 0, buffer.Length);

            uint output = (uint) IPAddress.NetworkToHostOrder((int) BitConverter.ToInt32(buffer));
            return output;
        }

        public void WriteUInt(uint input) => WriteUInt32(input);
        public uint ReadUInt() => ReadUInt32();

        public void WriteInt32(int input)
        {
            _EnsureReadyToWrite();

            byte[] bytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(input));
            _stream.Write(bytes, 0, bytes.Length);
        }

        public int ReadInt32()
        {
            byte[] buffer = new byte[4];
            int read = _stream.Read(buffer, 0, buffer.Length);

            int output = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(buffer));

            return output;
        }

        public void WriteInt(int input) => WriteInt32(input);
        public int ReadInt() => ReadInt32();

        public void WriteUInt64(ulong input)
        {
            _EnsureReadyToWrite();

            byte[] bytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((long) input));
            _stream.Write(bytes, 0, bytes.Length);
        }

        public ulong ReadUInt64()
        {
            byte[] buffer = new byte[8];
            int read = _stream.Read(buffer, 0, buffer.Length);

            ulong output = (ulong) IPAddress.NetworkToHostOrder(BitConverter.ToInt64(buffer));
            return output;
        }

        public void WriteULong(ulong input) => WriteUInt64(input);
        public ulong ReadULong() => ReadUInt64();

        public void WriteInt64(long input)
        {
            _EnsureReadyToWrite();

            byte[] bytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(input));
            _stream.Write(bytes, 0, bytes.Length);
        }

        public long ReadInt64()
        {
            byte[] buffer = new byte[8];
            int read = _stream.Read(buffer, 0, buffer.Length);

            long output = IPAddress.NetworkToHostOrder(BitConverter.ToInt64(buffer));
            return output;
        }

        public void WriteLong(long input) => WriteInt64(input);
        public long ReadLong() => ReadInt64();

        public void WriteFloat(float input)
        {
            _EnsureReadyToWrite();

            int i32 = (int)(FastMath.Round(input, FloatPrecision) * GetPrecisionNumber());
            WriteInt32(i32);
        }

        public void WriteSingle(float input) => WriteFloat(input);

        public float ReadFloat()
        {
            int i32 = ReadInt32();
            return i32 * _precision_reciprocal;
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
            return FastMath.Round(length / 1000, 3);
        }

    }

}