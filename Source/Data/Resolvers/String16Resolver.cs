///////////////////////////////////////////////////////
/// Filename: String16Resolver.cs
/// Date: August 28, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace EppNet.Data
{

    public class String16Resolver : Resolver<Str16>
    {

        public static readonly String16Resolver Instance = new();
        public const int LengthByteSize = sizeof(ushort);
        public String16Resolver() : base(autoAdvance: false) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override ReadResult _Internal_Read(BytePayload payload, out Str16 output)
        {
            Span<byte> buffer = stackalloc byte[LengthByteSize];
            int read = payload.Stream.Read(buffer);
            payload.Stream.Advance(read);
            
            bool didRead = BinaryPrimitives.TryReadUInt16LittleEndian(buffer, out ushort length);
            output = null;

            if (didRead)
            {
                buffer = stackalloc byte[length];
                read = payload.Stream.Read(buffer);
                output = payload.Encoder.GetString(buffer);
                payload.Stream.Advance(read);
            }

            return read == length ? ReadResult.Success : ReadResult.Failed;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool _Internal_Write(BytePayload payload, Str16 input)
        {
            bool writeable = !string.IsNullOrEmpty(input.Value);
            int length = payload.Encoder.GetByteCount(input.Value);

            bool written = BinaryPrimitives.TryWriteUInt16LittleEndian(
                payload.Stream.GetSpan(LengthByteSize), (ushort) length);

            if (written)
                payload.Stream.Advance(LengthByteSize);

            if (writeable && payload.Encoder.TryGetBytes(input.Value, 
                payload.Stream.GetSpan(length), out int bytes))
            {
                payload.Stream.Advance(bytes);
                return true;
            }

            return written;
        }

    }

    public static class String16ResolverExtensions
    {

        /// <summary>
        /// Writes an unsigned 16-bit integer denoting the length as well as
        /// the specified string in the encoding specified by <see cref="BytePayload.Encoder"/>
        /// to the stream.<br/>
        /// Writes are between 24 bytes and 524,296 bytes (~524.296KB) inclusive.
        /// </summary>
        /// <param name="input"></param>
        public static void Write(this BytePayload payload, Str16 input)
            => String16Resolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes an unsigned 16-bit integer denoting the length as well as
        /// the specified string array in the encoding specified by <see cref="BytePayload.Encoder"/>
        /// to the stream.<br/>
        /// Writes are between 24 bytes and 524,296 bytes (~524.296KB) inclusive.
        /// </summary>
        /// <param name="input"></param>
        public static void Write(this BytePayload payload, Str16[] input)
            => String16Resolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes an unsigned 16-bit integer denoting the length as well as
        /// the specified string array in the encoding specified by <see cref="BytePayload.Encoder"/>
        /// to the stream.<br/>
        /// Writes are between 24 bytes and 524,296 bytes (~524.296KB) inclusive.
        /// </summary>
        /// <param name="input"></param>
        public static void WriteArray(this BytePayload payload, Str16[] input)
            => String16Resolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes an unsigned 16-bit integer denoting the length as well as
        /// the specified string in the encoding specified by <see cref="BytePayload.Encoder"/>
        /// to the stream.<br/>
        /// Writes are between 24 bytes and 524,296 bytes (~524.296KB) inclusive.
        /// </summary>
        /// <param name="input"></param>

        public static void WriteString16(this BytePayload payload, Str16 input)
            => String16Resolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes an unsigned 16-bit integer denoting the length as well as
        /// the specified string array in the encoding specified by <see cref="BytePayload.Encoder"/>
        /// to the stream.<br/>
        /// Writes are between 24 bytes and 524,296 bytes (~524.296KB) inclusive.
        /// </summary>
        /// <param name="input"></param>
        public static void WriteString16Array(this BytePayload payload, string[] input)
        {
            UInt32Resolver.Instance.Write(payload, input.Length);

            for (int i = 0; i < input.Length; i++)
                String16Resolver.Instance.Write(payload, (Str16)input[i]);
        }

        /// <summary>
        /// Writes an unsigned 16-bit integer denoting the length as well as
        /// the specified string in the encoding specified by <see cref="BytePayload.Encoder"/>
        /// to the stream.<br/>
        /// Writes are between 24 bytes and 524,296 bytes (~524.296KB) inclusive.
        /// </summary>
        /// <param name="input"></param>

        public static void WriteString16(this BytePayload payload, string input)
            => String16Resolver.Instance.Write(payload, (Str16)input);

        /// <summary>
        /// Writes an unsigned 16-bit integer denoting the length as well as
        /// the specified string array in the encoding specified by <see cref="BytePayload.Encoder"/>
        /// to the stream.<br/>
        /// Writes are between 24 bytes and 524,296 bytes (~524.296KB) inclusive.
        /// </summary>
        /// <param name="input"></param>
        public static void WriteString16Array(this BytePayload payload, Str16[] input)
            => String16Resolver.Instance.Write(payload, input);

        /// <summary>
        /// Reads a string in the encoding specified by <see cref="BytePayload.Encoder"/>. <br/>
        /// See <see cref="WriteString16(BytePayload, Str16)"/> for more information on how this is
        /// written to the stream.
        /// </summary>
        /// <returns></returns>
        public static Str16 ReadString16(this BytePayload payload)
        {
            String16Resolver.Instance.Read(payload, out Str16 output);
            return output;
        }

        /// <summary>
        /// Reads a string array in the encoding specified by <see cref="BytePayload.Encoder"/>. <br/>
        /// See <see cref="WriteString16(BytePayload, Str16)"/> for more information on how this is
        /// written to the stream.
        /// </summary>
        /// <returns></returns>
        public static Str16[] ReadString16Array(this BytePayload payload)
        {
            String16Resolver.Instance.Read(payload, out Str16[] output);
            return output;
        }

        /// <summary>
        /// Reads a string array in the encoding specified by <see cref="BytePayload.Encoder"/>. <br/>
        /// See <see cref="WriteString16(BytePayload, Str16)"/> for more information on how this is
        /// written to the stream.
        /// </summary>
        /// <returns></returns>
        public static string[] ReadString16sAsSArray(this BytePayload payload)
        {
            UInt32Resolver.Instance.Read(payload, out uint length);
            string[] array = new string[length];

            for (uint i = 0; i < length; i++)
            {
                String16Resolver.Instance.Read(payload, out Str16 result);
                array[i] = result;
            }

            return array;
        }

    }

}

