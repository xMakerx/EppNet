///////////////////////////////////////////////////////
/// Filename: BytePayload.cs
/// Date: September 10, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////
/// <summary>
/// Performant I/O object for reading & writing C# primitives.
/// Leverages the functionality of Microsoft's <see cref="RecyclableMemoryStream"/>
/// and stack allocated spans to quickly read and write from a binary buffer.
/// While some functions such as #WriteTimestamp(<see cref="Timestamp"/>) exist, these objects
/// aren't serialized, they're broken down into primitives necessary
/// to reconstruct the object.
/// 
/// Binary data is exclusively written and read in little endian to
/// facilitate networking with computers with different CPUs.
/// </summary>


using EppNet.Time;
using EppNet.Exceptions;
using EppNet.Utilities;

using Microsoft.IO;

using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace EppNet.Data
{

    public class BytePayload : IDisposable
    {

        #region Static members
        public static RecyclableMemoryStreamManager RecyclableStreamMgr { private set; get; }

        private static readonly Dictionary<Type, IResolver> _resolvers = new Dictionary<Type, IResolver>();

        static BytePayload()
        {

            RecyclableMemoryStreamManager.Options options = new RecyclableMemoryStreamManager.Options();
            options.ZeroOutBuffer = false;
            options.BlockSize = 512; // 0.5 KB blocks
            options.LargeBufferMultiple = 2048; // Large buffers are multiples of 2 KB
            options.MaximumBufferSize = 12 * 1024 * 1024; // 12MB is too large to be repooled.
            options.UseExponentialLargeBuffer = false; // Linear growth strategy
            options.MaximumSmallPoolFreeBytes = 64 * 1024 * 1024; // Keep 64 MB worth of small pools around
            options.MaximumLargePoolFreeBytes = 128 * 1024 * 1024; // Keep 128MB worth of large pools around

            // Kills performance when enabled. Helpful in debugging situations
            options.GenerateCallStacks = false;

            RecyclableStreamMgr = new(options);

            AddResolver(typeof(Vector2), new Vector2Resolver());
            AddResolver(typeof(Vector3), new Vector3Resolver());
            AddResolver(typeof(Vector4), new Vector4Resolver());
        }

        public static void AddResolver(Type type, IResolver resolver)
        {
            _resolvers.Add(type, resolver);
        }

        public static IResolver GetResolver(Type type)
        {
            _resolvers.TryGetValue(type, out IResolver result);
            return result;
        }

        /// <summary>
        /// The decimal precision of floating point numbers.
        /// </summary>
        public static int FloatPrecision = 4;

        public static double PrecisionDecimalPlaces
        {
            get
            {
                if (_precisionDecimalPlaces == 0)
                    _precisionDecimalPlaces = FastMath.GetTenPow(FloatPrecision);

                return _precisionDecimalPlaces;
            }
        }

        private static double _precisionDecimalPlaces = 0d;

        public static double PrecisionReturnDecimalPlaces
        {
            get
            {
                if (_precisionReturnDecimalPlaces == 0)
                    _precisionReturnDecimalPlaces = 1 / PrecisionDecimalPlaces;

                return _precisionReturnDecimalPlaces;
            }
        }

        private static double _precisionReturnDecimalPlaces = 0d;

        public static RecyclableMemoryStream ObtainStream() => RecyclableStreamMgr.GetStream();

        public static RecyclableMemoryStream ObtainStream(byte[] buffer) => RecyclableStreamMgr.GetStream(buffer);

        /// <summary>
        /// Converts a local float to a remote float (i.e. a float with the precision specified by <see cref="FloatPrecision"/>)
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>

        public static float FloatToNetFloat(float input)
        {
            int a = (int)(input.Round(FloatPrecision) * PrecisionDecimalPlaces);
            return a * (float) PrecisionReturnDecimalPlaces;
        }

        #endregion

        /// <summary>
        /// The encoding this payload uses when handling strings.
        /// <br/>Default is <see cref="Encoding.UTF8"/><br/><br/>
        /// Do not change the encoder after you've already begun I/O operations
        /// on this payload as that's unexpected behavior.
        /// </summary>

        public Encoding Encoder = Encoding.UTF8;

        /// <summary>
        /// A byte array representing the data held by the internal <see cref="RecyclableMemoryStream"/>.<br/>
        /// This is populated by <see cref="Pack"/>.
        /// </summary>
        public byte[] PackedData { internal set; get; }

        /// <summary>
        /// The length of this payload in bytes
        /// </summary>

        public long Length
        {
            get
            {
                return _stream != null ? _stream.Length : 0;
            }
        }

        protected internal RecyclableMemoryStream _stream;

        /// <summary>
        /// Initializes an empty new BytePayload. A memory stream is not fetched from the pool
        /// until a write method is called.
        /// </summary>

        public BytePayload()
        {
            this.PackedData = null;
            this._stream = null;
        }

        /// <summary>
        /// Initializes a new BytePayload with the provided byte array allocated into an obtained
        /// <see cref="RecyclableMemoryStream"/>
        /// </summary>
        /// <param name="dataIn"></param>

        public BytePayload(byte[] dataIn) : this()
        {
            _stream = ObtainStream(dataIn);
            _stream.Seek(0, System.IO.SeekOrigin.Begin);
        }

        public bool Advance(int byteLength)
        {
            if (_stream == null)
                return false;

            _stream.Advance(byteLength);
            return true;
        }

        /// <summary>
        /// Tries to write an object of unknown type.
        /// Returns true if a successful companion write function was called or
        /// false if unable to find the correct function.
        /// </summary>

        public virtual bool TryWrite(object input)
        {

            switch (Type.GetTypeCode(input.GetType()))
            {
                case TypeCode.String:
                    // We didn't use a custom string type
                    return false;

                case TypeCode.Boolean:
                    WriteBool((bool)input);
                    break;

                case TypeCode.Byte:
                    WriteByte((byte)input);
                    break;

                case TypeCode.SByte:
                    WriteSByte((sbyte)input);
                    break;

                case TypeCode.Int16:
                    WriteInt16((short)input);
                    break;

                case TypeCode.UInt16:
                    WriteUInt16((ushort)input);
                    break;

                case TypeCode.Int32:
                    WriteInt32((int)input);
                    break;

                case TypeCode.UInt32:
                    WriteUInt32((uint)input);
                    break;

                case TypeCode.Int64:
                    WriteInt64((long)input);
                    break;

                case TypeCode.UInt64:
                    WriteUInt64((ulong)input);
                    break;

                case TypeCode.Single:
                    WriteSingle((float)input);
                    break;

                case TypeCode.Object:
                    // Try to resolve the type to either a string type or
                    // a type we have a resolver for

                    if (input.GetType() == typeof(Str16))
                    {
                        WriteString16((Str16)input);
                        break;
                    }    

                    if (input.GetType() == typeof(Str8))
                    {
                        WriteString8((Str8)input);
                        break;
                    }

                    IResolver resolver = GetResolver(input.GetType());
                    return resolver?.Write(this, input) == true;

                default:
                    return false;

            }

            return true;
        }

        /// <summary>
        /// Locates the proper read function for the specified type, calls it,
        /// and returns the result.
        /// If no companion function is located to call, null is returned.
        /// </summary>
        /// <param name="type"></param>
        /// <returns>Result of companion #Read?() function or null</returns>

        public virtual object TryRead(Type type)
        {

            object output = null;

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.String:
                    // Must explicitly specify a string type!
                    return null;

                case TypeCode.Boolean:
                    output = ReadBool();
                    break;

                case TypeCode.Byte:
                    output = ReadByte();
                    break;

                case TypeCode.SByte:
                    output = ReadSByte();
                    break;

                case TypeCode.Int16:
                    output = ReadInt16();
                    break;

                case TypeCode.UInt16:
                    output = ReadUInt16();
                    break;

                case TypeCode.Int32:
                    output = ReadInt32();
                    break;

                case TypeCode.UInt32:
                    output = ReadUInt32();
                    break;

                case TypeCode.Int64:
                    output = ReadInt64();
                    break;

                case TypeCode.UInt64:
                    output = ReadUInt64();
                    break;

                case TypeCode.Single:
                    output = ReadSingle();
                    break;

                case TypeCode.Object:

                    if (type == typeof(Str16))
                        return ReadString16();
                    else if (type == typeof(Str8))
                        return ReadString8();

                    // Try to resolve the type
                    IResolver resolver = GetResolver(type);
                    resolver?.Read(this, out output);

                    break;

            }

            return output;
        }

        /// <summary>
        /// Packages the data into a byte array to be send on the wire.
        /// <br/>It's recommended to only call this when you're completely done writing data.
        /// </summary>
        /// <returns></returns>

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

        public virtual void Reset()
        {

            bool hadStream = _stream != null;

            Dispose();

            if (hadStream)
                EnsureReadyToWrite();
        }

        /// <summary>
        /// Disposes of the internal <see cref="RecyclableMemoryStream"/>.
        /// <br/>DO NOT call unless you're sure you're done reading and writing from this payload.
        /// </summary>

        public virtual void Dispose()
        {
            _stream?.Dispose();
            _stream = null;

            PackedData = null;
        }

        /// <summary>
        /// Reads a string in the encoding specified by <see cref="Encoder"/>. <br/>
        /// Only call this if you know the exact length of the string!
        /// </summary>
        /// <returns></returns>

        public string ReadString(int length)
        {
            Span<byte> buffer = stackalloc byte[length];
            int read = _stream.Read(buffer);

            return Encoder.GetString(buffer);
        }

        /// <summary>
        /// Writes an unsigned 8-bit integer denoting the length as well as
        /// the specified string in the encoding specified by <see cref="Encoder"/>
        /// to the stream.<br/>
        /// Writes are between 16 bytes and 2,048 bytes inclusive.
        /// </summary>
        /// <param name="input"></param>

        public void WriteString8(string input)
        {
            if (input.Length == 0 || input.Length > byte.MaxValue)
                throw new ArgumentOutOfRangeException($"BytePayload#WriteString8(string) must be between 1 and {byte.MaxValue}.");

            EnsureReadyToWrite();

            WriteUInt8((byte)input.Length);
            _Internal_WriteString(input);
        }

        /// <summary>
        /// Reads a string in the encoding specified by <see cref="Encoder"/>. <br/>
        /// See <see cref="WriteString8(string)"/> for more information on how this is
        /// written to the stream.
        /// </summary>
        /// <returns></returns>

        public string ReadString8()
        {
            int length = ReadUInt8();
            return ReadString(length);
        }

        /// <summary>
        /// Writes an unsigned 16-bit integer denoting the length as well as
        /// the specified string in the encoding specified by <see cref="Encoder"/>
        /// to the stream.<br/>
        /// Writes are between 24 bytes and 524,296 bytes (~524.296KB) inclusive.
        /// </summary>
        /// <param name="input"></param>

        public void WriteString16(string input)
        {
            if (input.Length == 0 || input.Length > ushort.MaxValue)
                throw new ArgumentOutOfRangeException($"BytePayload#WriteString16(string) must be between 1 and {ushort.MaxValue}.");

            EnsureReadyToWrite();

            WriteUInt16((ushort)input.Length);
            _Internal_WriteString(input);
        }

        /// <summary>
        /// Reads a string in the encoding specified by <see cref="Encoder"/>. <br/>
        /// See <see cref="WriteString16(string)"/> for more information on how this is
        /// written to the stream.
        /// </summary>
        /// <returns></returns>

        public string ReadString16()
        {
            int length = ReadUInt16();
            return ReadString(length);
        }

        /// <summary>
        /// Writes an unsigned 8-bit integer to the stream denoting 1 (true) or 0 (false).
        /// </summary>
        /// <param name="input"></param>

        public void WriteBool(bool input)
        {
            EnsureReadyToWrite();
            WriteUInt8((byte) (input ? 1 : 0));
        }

        /// <summary>
        /// Reads an unsigned 8-bit integer from the stream and checks if it equals 1 or 0.<br/>
        /// <see cref="WriteBool(bool)"/>
        /// </summary>
        /// <returns></returns>

        public bool ReadBool() => (ReadUInt8() == 1);

        /// <summary>
        /// Writes an unsigned 8-bit integer to the stream.
        /// </summary>
        /// <param name="input"></param>

        public void WriteUInt8(byte input)
        {
            EnsureReadyToWrite();
            _stream.WriteByte(input);
        }

        /// <summary>
        /// Reads an unsigned 8-bit integer from the stream.
        /// <br/>
        /// <br/>
        /// Exceptions: <br/>
        /// - <see cref="BytePayloadReadException"/> Out of Range
        /// </summary>

        public byte ReadUInt8()
        {
            long pos = _stream.Position;
            int result = _stream.ReadByte();

            if (result == -1)
            {
                // This is the end of the stream.
                throw BytePayloadReadException.NewOutOfRangeException(this, pos, 1);
            }

            return (byte)result;
        }

        /// <summary>
        /// Writes an unsigned 8-bit integer to the stream.
        /// </summary>
        /// <param name="input"></param>

        public void WriteByte(byte input) => WriteUInt8(input);

        /// <summary>
        /// Reads an unsigned 8-bit integer from the stream.
        /// </summary>
        public byte ReadByte() => ReadUInt8();

        /// <summary>
        /// Writes an 8-bit integer to the stream.
        /// </summary>
        /// <param name="input"></param>

        public void WriteInt8(sbyte input)
        {
            EnsureReadyToWrite();
            _stream.WriteByte((byte)input);
        }

        /// <summary>
        /// Reads an 8-bit integer from the stream.
        /// <br/>
        /// <br/>
        /// Exceptions: <br/>
        /// - <see cref="BytePayloadReadException"/> Out of Range
        /// </summary>

        public sbyte ReadInt8()
        {
            long pos = _stream.Position;
            int result = _stream.ReadByte();

            if (result == -1)
            {
                // This is the end of the stream.
                throw BytePayloadReadException.NewOutOfRangeException(this, pos, 1);
            }

            return (sbyte)result;
        }

        /// <summary>
        /// Writes an 8-bit integer to the stream.
        /// </summary>
        /// <param name="input"></param>

        public void WriteSByte(sbyte input) => WriteInt8(input);

        /// <summary>
        /// Reads an 8-bit integer from the stream.
        /// </summary>
        public sbyte ReadSByte() => ReadInt8();

        /// <summary>
        /// Writes a 16-bit integer to the stream.
        /// </summary>
        /// <param name="input"></param>

        public void WriteInt16(short input)
        {
            EnsureReadyToWrite();
            BinaryPrimitives.WriteInt16LittleEndian(_stream.GetSpan(2), input);
            _stream.Advance(2);
        }

        /// <summary>
        /// Reads a 16-bit integer from the stream.
        /// </summary>

        public short ReadInt16()
        {
            Span<byte> buffer = stackalloc byte[2];
            int read = _stream.Read(buffer);
            short output = BinaryPrimitives.ReadInt16LittleEndian(buffer);
            return output;
        }

        /// <summary>
        /// Writes a 16-bit integer to the stream.
        /// </summary>
        /// <param name="input"></param>

        public void WriteShort(short input) => WriteInt16(input);

        /// <summary>
        /// Reads a 16-bit integer from the stream.
        /// </summary>
        public short ReadShort() => ReadInt16();

        /// <summary>
        /// Writes an unsigned 16-bit integer to the stream.
        /// </summary>
        /// <param name="input"></param>

        public void WriteUInt16(ushort input)
        {
            EnsureReadyToWrite();

            BinaryPrimitives.WriteUInt16LittleEndian(_stream.GetSpan(2), input);
            _stream.Advance(2);
        }

        /// <summary>
        /// Reads an unsigned 16-bit integer from the stream.
        /// </summary>

        public ushort ReadUInt16()
        {
            Span<byte> buffer = stackalloc byte[2];
            int read = _stream.Read(buffer);
            ushort output = BinaryPrimitives.ReadUInt16LittleEndian(buffer);

            return output;
        }

        /// <summary>
        /// Writes an unsigned 16-bit integer to the stream.
        /// </summary>
        /// <param name="input"></param>

        public void WriteUShort(ushort input) => WriteUInt16(input);

        /// <summary>
        /// Reads an unsigned 16-bit integer from the stream.
        /// </summary>

        public ushort ReadUShort() => ReadUInt16();

        /// <summary>
        /// Writes an unsigned 32-bit integer to the stream.
        /// </summary>
        /// <param name="input"></param>

        public void WriteUInt32(uint input)
        {
            EnsureReadyToWrite();

            BinaryPrimitives.WriteUInt32LittleEndian(_stream.GetSpan(4), input);
            _stream.Advance(4);
        }

        /// <summary>
        /// Reads an unsigned 32-bit integer from the stream.
        /// </summary>

        public uint ReadUInt32()
        {
            Span<byte> buffer = stackalloc byte[4];
            int read = _stream.Read(buffer);
            uint output = BinaryPrimitives.ReadUInt32LittleEndian(buffer);

            return output;
        }

        /// <summary>
        /// Writes an unsigned 32-bit integer to the stream.
        /// </summary>
        /// <param name="input"></param>

        public void WriteUInt(uint input) => WriteUInt32(input);

        /// <summary>
        /// Reads an unsigned 32-bit integer from the stream.
        /// </summary>

        public uint ReadUInt() => ReadUInt32();

        /// <summary>
        /// Writes a 32-bit integer to the stream.
        /// </summary>
        /// <param name="input"></param>

        public void WriteInt32(int input)
        {
            EnsureReadyToWrite();

            BinaryPrimitives.WriteInt32LittleEndian(_stream.GetSpan(4), input);
            _stream.Advance(4);
        }

        /// <summary>
        /// Reads a 32-bit integer from the stream.
        /// </summary>

        public int ReadInt32()
        {
            Span<byte> buffer = stackalloc byte[4];
            int read = _stream.Read(buffer);
            int output = BinaryPrimitives.ReadInt32LittleEndian(buffer);

            return output;
        }

        /// <summary>
        /// Writes a 32-bit integer to the stream.
        /// </summary>
        /// <param name="input"></param>

        public void WriteInt(int input) => WriteInt32(input);

        /// <summary>
        /// Reads a 32-bit integer from the stream.
        /// </summary>
        public int ReadInt() => ReadInt32();

        /// <summary>
        /// Writes an unsigned 64-bit integer to the stream.
        /// </summary>
        /// <param name="input"></param>

        public void WriteUInt64(ulong input)
        {
            EnsureReadyToWrite();

            BinaryPrimitives.WriteUInt64LittleEndian(_stream.GetSpan(8), input);
            _stream.Advance(8);
        }

        /// <summary>
        /// Reads an unsigned 64-bit integer from the stream.
        /// </summary>

        public ulong ReadUInt64()
        {
            Span<byte> buffer = stackalloc byte[8];
            int read = _stream.Read(buffer);
            ulong output = BinaryPrimitives.ReadUInt64LittleEndian(buffer);

            return output;
        }

        /// <summary>
        /// Writes an unsigned 64-bit integer to the stream.
        /// </summary>
        /// <param name="input"></param>

        public void WriteULong(ulong input) => WriteUInt64(input);

        /// <summary>
        /// Reads an unsigned 64-bit integer from the stream.
        /// </summary>

        public ulong ReadULong() => ReadUInt64();

        /// <summary>
        /// Writes a 64-bit integer to the stream.
        /// </summary>
        /// <param name="input"></param>

        public void WriteInt64(long input)
        {
            EnsureReadyToWrite();

            BinaryPrimitives.WriteInt64LittleEndian(_stream.GetSpan(8), input);
            _stream.Advance(8);
        }

        /// <summary>
        /// Reads a 64-bit integer from the stream.
        /// </summary>

        public long ReadInt64()
        {
            Span<byte> buffer = stackalloc byte[8];
            int read = _stream.Read(buffer);
            long output = BinaryPrimitives.ReadInt64LittleEndian(buffer);

            return output;
        }

        /// <summary>
        /// Writes a 64-bit integer to the stream.
        /// </summary>
        /// <param name="input"></param>

        public void WriteLong(long input) => WriteInt64(input);

        /// <summary>
        /// Reads a 64-bit integer from the stream.
        /// </summary>

        public long ReadLong() => ReadInt64();

        /// <summary>
        /// Writes a provided float input as a 32-bit integer.<br/>
        /// Floats are sent with the precision specified by <see cref="FloatPrecision"/>
        /// </summary>
        /// <param name="input"></param>

        public void WriteFloat(float input)
        {
            EnsureReadyToWrite();

            double rounded = input.Round(FloatPrecision);

            int i32 = (int)(rounded * PrecisionDecimalPlaces);
            WriteInt32(i32);
        }

        /// <summary>
        /// Writes a provided float input as a 32-bit integer.<br/>
        /// Floats are sent with the precision specified by <see cref="FloatPrecision"/>
        /// </summary>
        /// <param name="input"></param>

        public void WriteSingle(float input) => WriteFloat(input);

        /// <summary>
        /// Reads a float from the stream.<br/>
        /// Floats are sent with the precision specified by <see cref="FloatPrecision"/>
        /// </summary>
        /// <returns></returns>

        public float ReadFloat()
        {
            float i32 = ReadInt32();
            return (float) (i32 * PrecisionReturnDecimalPlaces);
        }

        /// <summary>
        /// Reads a float from the stream.<br/>
        /// Floats are sent with the precision specified by <see cref="FloatPrecision"/>
        /// </summary>
        /// <returns></returns>

        public float ReadSingle() => ReadFloat();

        /// <summary>
        /// Writes a timestamp to the wire including its unsigned 8-bit integer type and
        /// 64-bit integer denoting its value.
        /// </summary>

        public void WriteTypedTimestamp(Timestamp input)
        {
            EnsureReadyToWrite();

            WriteUInt8((byte)input.Type);
            WriteLong(input.Value);
        }

        /// <summary>
        /// Reads a 64-integer and creates a <see cref="Timestamp"/> with
        /// the received time and type.
        /// </summary>
        /// <returns></returns>

        public Timestamp ReadTypedTimestamp()
        {
            byte type = ReadUInt8();
            long value = ReadLong();

            TimestampType tsType = (TimestampType)Enum.ToObject(typeof(TimestampType), type);
            return new Timestamp(tsType, false, value);
        }

        /// <summary>
        /// Writes a 64-bit integer denoting the value of the specified <see cref="Timestamp"/>.
        /// </summary>

        public void WriteTimestamp(Timestamp input)
        {
            EnsureReadyToWrite();

            WriteLong(input.Value);
        }

        /// <summary>
        /// Reads a 64-integer and creates a <see cref="TimestampType.None"/> <see cref="Timestamp"/> with
        /// the received time.
        /// </summary>
        /// <returns></returns>

        public Timestamp ReadTimestamp()
        {
            long value = ReadLong();
            return new Timestamp() { Value = value };
        }

        /// <summary>
        /// Returns the size of this payload in kilobytes.
        /// </summary>
        /// <returns></returns>

        public double GetSizeKB()
        {
            float length = (_stream != null) ? _stream.Length : 0;
            return (length * 0.001).Round(3);
        }

        /// <summary>
        /// Ensures the internal <see cref="RecyclableMemoryStream"/> is ready to be written to.
        /// </summary>

        public virtual void EnsureReadyToWrite()
        {
            _stream ??= ObtainStream();
        }

        /// <summary>
        /// Directly writes a string to the stream. Call AFTER writing the length
        /// of the string being written. See <see cref="WriteString8(string)"/> and <see cref="WriteString16(string)"/>
        /// for the intended approach. <br/>If your string is larger than 65,535 characters, you should reconsider what
        /// you're doing as it's incredibly wasteful to send a string that large over the wire.
        /// </summary>
        /// <param name="input"></param>

        protected void _Internal_WriteString(string input)
        {
            Span<byte> span = _stream.GetSpan(input.Length);
            ReadOnlySequence<char> chars = new(input.AsMemory());

            int written = Encoder.GetBytes(chars, span);
            _stream.Advance(written);
        }

    }

}