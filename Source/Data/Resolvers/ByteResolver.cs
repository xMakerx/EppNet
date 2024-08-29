///////////////////////////////////////////////////////
/// Filename: ByteResolver.cs
/// Date: August 28, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System.Runtime.CompilerServices;

namespace EppNet.Data
{

    public class ByteResolver : Resolver<byte>
    {

        public static readonly ByteResolver Instance = new();

        static ByteResolver()
            => BytePayload.AddResolver(typeof(byte), Instance);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool _Internal_Read(BytePayload payload, out byte output)
        {
            int result = payload.Stream.ReadByte();
            output = (byte)result;

            return result != -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool _Internal_Write(BytePayload payload, byte input)
        {
            payload.Stream.WriteByte(input);
            return true;
        }

    }

    public static class ByteResolverExtensions
    {

        /// <summary>
        /// Writes an unsigned 8-bit integer to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void Write(this BytePayload payload, byte input)
            => ByteResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes an unsigned 8-bit integer array to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void Write(this BytePayload payload, byte[] input)
            => ByteResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes an unsigned 8-bit integer array to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void WriteArray(this BytePayload payload, byte[] input)
            => ByteResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes an unsigned 8-bit integer to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void WriteByte(this BytePayload payload, byte input)
            => ByteResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes an unsigned 8-bit integer array to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void WriteByteArray(this BytePayload payload, byte[] input)
            => ByteResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes an unsigned 8-bit integer to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void WriteUInt8(this BytePayload payload, byte input)
            => ByteResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes an unsigned 8-bit integer array to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void WriteUInt8Array(this BytePayload payload, byte[] input)
            => ByteResolver.Instance.Write(payload, input);

        /// <summary>
        /// Reads an unsigned 8-bit integer from the stream.
        /// </summary>
        public static byte ReadByte(this BytePayload payload)
        {
            ByteResolver.Instance.Read(payload, out byte output);
            return output;
        }

        /// <summary>
        /// Reads an unsigned 8-bit integer array from the stream.
        /// </summary>
        public static byte[] ReadByteArray(this BytePayload payload)
        {
            ByteResolver.Instance.Read(payload, out byte[] output);
            return output;
        }

        /// <summary>
        /// Reads an unsigned 8-bit integer from the stream.
        /// </summary>
        public static byte ReadUInt8(this BytePayload payload)
        {
            ByteResolver.Instance.Read(payload, out byte output);
            return output;
        }

        /// <summary>
        /// Reads an unsigned 8-bit integer array from the stream.
        /// </summary>
        public static byte[] ReadUInt8Array(this BytePayload payload)
        {
            ByteResolver.Instance.Read(payload, out byte[] output);
            return output;
        }
    }

}
