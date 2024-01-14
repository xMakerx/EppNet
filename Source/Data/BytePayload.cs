///////////////////////////////////////////////////////
/// Filename: BytePayload.cs
/// Date: September 10, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////
/// Performant I/O object for reading & writing C# primitives.
/// Leverages the functionality of Microsoft's <see cref="RecyclableMemoryStream"/>
/// and stack allocated spans to quickly read and write from a binary buffer.
/// While some functions such as #WriteTimestamp(<see cref="Timestamp"/>) exist, these objects
/// aren't serialized, they're broken down into the necessary primitives necessary
/// to reconstruct the object.
/// 
/// Binary data is exclusively written and read in little endian to
/// facilitate networking with computers with different CPUs.


using EppNet.Core;
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
            RecyclableStreamMgr = new RecyclableMemoryStreamManager(
                512, // 0.5KB blocks
                2048, // Large buffers are multiples of 2KB
                12 * 1024 * 1024, // 12MB is too large to be repooled
                false, // Use linear growth
                64 * 1024 * 1024, // 64MB is the max for small pools,
                128 * 1024 * 1024 // 128MB is the max for large pools
            );

            // Kills performance when enabled.
            RecyclableStreamMgr.GenerateCallStacks = false;

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
        /// <param name="input"></param>
        /// <returns></returns>

        public virtual bool TryWrite(object input)
        {

            if (input is Str16)
            {
                WriteString16((Str16)input);
                return true;
            }

            if (input is Str8)
            {
                WriteString8((Str8)input);
                return true;
            }

            if (input is bool)
            {
                WriteBool((bool)input);
                return true;
            }

            if (input is byte)
            {
                WriteByte((byte)input);
                return true;
            }

            if (input is sbyte)
            {
                WriteSByte((sbyte)input);
                return true;
            }

            if (input is short)
            {
                WriteShort((short)input);
                return true;
            }

            if (input is ushort)
            {
                WriteUShort((ushort)input);
                return true;
            }

            if (input is int)
            {
                WriteInt((int)input);
                return true;
            }

            if (input is uint)
            {
                WriteUInt((uint)input);
                return true;
            }

            if (input is long)
            {
                WriteLong((long)input);
                return true;
            }

            if (input is ulong)
            {
                WriteULong((ulong)input);
                return true;
            }

            if (input is float)
            {
                WriteFloat((float)input);
                return true;
            }

            IResolver resolver = GetResolver(input.GetType());

            if (resolver != null)
            {
                resolver.Write(this, input);
                return true;
            }

            return false;
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
            if (type == typeof(Str16))
                return ReadString16();

            if (type == typeof(Str8))
                return ReadString8();

            if (type == typeof(bool))
                return ReadBool();

            if (type == typeof(byte))
                return ReadByte();

            if (type == typeof(sbyte))
                return ReadSByte();

            if (type == typeof(short))
                return ReadShort();

            if (type == typeof(ushort))
                return ReadUShort();

            if (type == typeof(int))
                return ReadInt();

            if (type == typeof(uint))
                return ReadUInt();

            if (type == typeof(long))
                return ReadLong();

            if (type == typeof(ulong))
                return ReadULong();

            if (type == typeof(float))
                return ReadFloat();

            IResolver resolver = GetResolver(type);

            if (resolver != null)
            {
                resolver.Read(this, out object output);
                return output;
            }

            return null;
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
            if (_stream != null)
            {
                //byte[] buffer = _stream.GetBuffer();
                //Array.Clear(buffer, 0, buffer.Length);
                _stream.SetLength(0);
            }

            PackedData = null;
        }

        /// <summary>
        /// Disposes of the internal <see cref="RecyclableMemoryStream"/>.
        /// <br/>DO NOT call unless you're sure you're done reading and writing from this payload.
        /// </summary>

        public virtual void Dispose()
        {
            _stream?.Dispose();
            _stream = null;
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

            /*
            Span<byte> buffer = stackalloc byte[1] { input };
            _stream.Write(buffer);*/
        }

        /// <summary>
        /// Reads an unsigned 8-bit integer from the stream.
        /// </summary>
        /// <param name="input"></param>

        public byte ReadUInt8()
        {
            /*
            Span<byte> buffer = stackalloc byte[1];
            int read = _stream.Read(buffer);

            byte output = buffer[0];*/
            byte output = (byte)_stream.ReadByte();
            return output;
        }

        /// <summary>
        /// Writes an unsigned 8-bit integer to the stream.
        /// </summary>
        /// <param name="input"></param>

        public void WriteByte(byte input) => WriteUInt8(input);

        /// <summary>
        /// Reads an unsigned 8-bit integer from the stream.
        /// </summary>
        /// <param name="input"></param>
        public byte ReadByte() => ReadUInt8();

        /// <summary>
        /// Writes an 8-bit integer to the stream.
        /// </summary>
        /// <param name="input"></param>

        public void WriteInt8(sbyte input)
        {
            EnsureReadyToWrite();
            _stream.WriteByte((byte)input);

            /*

            Span<byte> span = _stream.GetSpan(sizeof(sbyte));
            MemoryMarshal.Write(span, ref input);
            _stream.Advance(sizeof(sbyte));*/
        }

        /// <summary>
        /// Reads an 8-bit integer from the stream.
        /// </summary>
        /// <param name="input"></param>

        public sbyte ReadInt8()
        {
            /*
            Span<byte> buffer = stackalloc byte[sizeof(sbyte)];
            int read = _stream.Read(buffer);
            sbyte output = unchecked((sbyte)buffer[0]);*/
            sbyte output = (sbyte)_stream.ReadByte();
            return output;
        }

        /// <summary>
        /// Writes an 8-bit integer to the stream.
        /// </summary>
        /// <param name="input"></param>

        public void WriteSByte(sbyte input) => WriteInt8(input);

        /// <summary>
        /// Reads an 8-bit integer from the stream.
        /// </summary>
        /// <param name="input"></param>
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
        /// <param name="input"></param>

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
        /// <param name="input"></param>
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
        /// <param name="input"></param>

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
        /// <param name="input"></param>

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
        /// <param name="input"></param>

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
        /// <param name="input"></param>

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
        /// <param name="input"></param>

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
        /// <param name="input"></param>
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
        /// <param name="input"></param>

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
        /// <param name="input"></param>

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
        /// <param name="input"></param>

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
        /// <param name="input"></param>

        public long ReadLong() => ReadInt64();

        /// <summary>
        /// Writes a provided float input as a 32-bit integer.<br/>
        /// Floats are sent with the precision specified by <see cref="FloatPrecision"/>
        /// </summary>
        /// <param name="input"></param>

        public void WriteFloat(float input)
        {
            EnsureReadyToWrite();

            int i32 = (int) (FastMath.Round(input, FloatPrecision) * FastMath.GetTenPow(FloatPrecision));
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
            float i32 = (float) ReadInt32();
            return i32 / (float) FastMath.GetTenPow(FloatPrecision);
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
        /// <param name="input"></param>

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
        /// <param name="input"></param>

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
            return FastMath.Round(length * 0.001, 3);
        }

        /// <summary>
        /// Ensures the internal <see cref="RecyclableMemoryStream"/> is ready to be written to.
        /// </summary>

        public virtual void EnsureReadyToWrite()
        {
            if (_stream == null)
                _stream = RecyclableStreamMgr.GetStream() as RecyclableMemoryStream;
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