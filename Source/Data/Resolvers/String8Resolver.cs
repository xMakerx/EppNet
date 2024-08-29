///////////////////////////////////////////////////////
/// Filename: String8Resolver.cs
/// Date: August 28, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;
using System.Runtime.CompilerServices;

namespace EppNet.Data
{

    public class String8Resolver : Resolver<Str8>
    {

        public static readonly String8Resolver Instance = new();

        public String8Resolver() : base(autoAdvance: false) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool _Internal_Read(BytePayload payload, out Str8 output)
        {
            int length = payload.Stream.ReadByte();
            payload.Stream.Advance(1);
            output = new(string.Empty);

            if (length < 0)
                // End of stream
                return false;
            else if (length == 0)
                // Empty string
                return true;

            Span<byte> buffer = stackalloc byte[length];
            int read = payload.Stream.Read(buffer);
            output = payload.Encoder.GetString(buffer);
            return read == length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool _Internal_Write(BytePayload payload, Str8 input)
        {
            bool writeable = !string.IsNullOrEmpty(input.Value);
            int numBytes = payload.Encoder.GetByteCount(input.Value);

            payload.Stream.WriteByte((byte) numBytes);
            payload.Advance(sizeof(byte));

            if (writeable && payload.Encoder.TryGetBytes(input.Value, payload.Stream.GetSpan(numBytes), out int bytes))
            {
                payload.Stream.Advance(bytes);
                return true;
            }

            return false;
        }

    }

    public static class String8ResolverExtensions
    {

        /// <summary>
        /// Writes an unsigned 8-bit integer denoting the length as well as
        /// the specified string in the encoding specified by <see cref="BytePayload.Encoder"/>
        /// to the stream.<br/>
        /// Writes are between 16 bytes and 2,048 bytes inclusive.
        /// </summary>
        /// <param name="input"></param>

        public static void Write(this BytePayload payload, Str8 input)
            => String8Resolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes an unsigned 8-bit integer denoting the length as well as
        /// the specified string in the encoding specified by <see cref="BytePayload.Encoder"/>
        /// to the stream.<br/>
        /// Writes are between 16 bytes and 2,048 bytes inclusive.
        /// </summary>
        /// <param name="input"></param>
        public static void Write(this BytePayload payload, Str8[] input)
            => String8Resolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes an unsigned 8-bit integer denoting the length as well as
        /// the specified string in the encoding specified by <see cref="BytePayload.Encoder"/>
        /// to the stream.<br/>
        /// Writes are between 16 bytes and 2,048 bytes inclusive.
        /// </summary>
        /// <param name="input"></param>
        public static void WriteArray(this BytePayload payload, Str8[] input)
            => String8Resolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes an unsigned 8-bit integer denoting the length as well as
        /// the specified string in the encoding specified by <see cref="BytePayload.Encoder"/>
        /// to the stream.<br/>
        /// Writes are between 16 bytes and 2,048 bytes inclusive.
        /// </summary>
        /// <param name="input"></param>

        public static void WriteString8(this BytePayload payload, string input)
            => String8Resolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes an unsigned 8-bit integer denoting the length as well as
        /// the specified string in the encoding specified by <see cref="BytePayload.Encoder"/>
        /// to the stream.<br/>
        /// Writes are between 16 bytes and 2,048 bytes inclusive.
        /// </summary>
        /// <param name="input"></param>
        public static void WriteString8Array(this BytePayload payload, string[] input)
        {
            UInt32Resolver.Instance.Write(payload, input.Length);

            for (int i = 0; i < input.Length; i++)
                String8Resolver.Instance.Write(payload, (Str8) input[i]);
        }

        /// <summary>
        /// Writes an unsigned 8-bit integer denoting the length as well as
        /// the specified string in the encoding specified by <see cref="BytePayload.Encoder"/>
        /// to the stream.<br/>
        /// Writes are between 16 bytes and 2,048 bytes inclusive.
        /// </summary>
        /// <param name="input"></param>
        public static void WriteString8(this BytePayload payload, Str8 input)
            => String8Resolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes an unsigned 8-bit integer denoting the length as well as
        /// the specified string in the encoding specified by <see cref="BytePayload.Encoder"/>
        /// to the stream.<br/>
        /// Writes are between 16 bytes and 2,048 bytes inclusive.
        /// </summary>
        /// <param name="input"></param>
        public static void WriteString8Array(this BytePayload payload, Str8[] input)
            => String8Resolver.Instance.Write(payload, input);

        /// <summary>
        /// Reads a string in the encoding specified by <see cref="BytePayload.Encoder"/>. <br/>
        /// See <see cref="WriteString8(BytePayload, string)"/> for more information on how this is
        /// written to the stream.
        /// </summary>
        /// <returns></returns>
        public static Str8 ReadString8(this BytePayload payload)
        {
            String8Resolver.Instance.Read(payload, out Str8 output);
            return output;
        }

        /// <summary>
        /// Reads a string in the encoding specified by <see cref="BytePayload.Encoder"/>. <br/>
        /// See <see cref="WriteString8(BytePayload, string)"/> for more information on how this is
        /// written to the stream.
        /// </summary>
        /// <returns></returns>
        public static Str8[] ReadString8Array(this BytePayload payload)
        {
            String8Resolver.Instance.Read(payload, out Str8[] output);
            return output;
        }

        /// <summary>
        /// Reads a string in the encoding specified by <see cref="BytePayload.Encoder"/>. <br/>
        /// See <see cref="WriteString8(BytePayload, string)"/> for more information on how this is
        /// written to the stream.
        /// </summary>
        /// <returns></returns>
        public static string[] ReadString8sAsSArray(this BytePayload payload)
        {
            UInt32Resolver.Instance.Read(payload, out uint length);
            string[] array = new string[length];

            for (uint i = 0; i < length; i++)
            {
                String8Resolver.Instance.Read(payload, out Str8 result);
                array[i] = result;
            }

            return array;
        }

    }

}

