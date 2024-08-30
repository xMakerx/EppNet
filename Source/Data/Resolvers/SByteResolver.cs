///////////////////////////////////////////////////////
/// Filename: SByteResolver.cs
/// Date: August 28, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System.Runtime.CompilerServices;

namespace EppNet.Data
{

    public class SByteResolver : Resolver<sbyte>
    {

        public static readonly SByteResolver Instance = new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override ReadResult _Internal_Read(BytePayload payload, out sbyte output)
        {
            int result = payload.Stream.ReadByte();
            output = (sbyte)result;

            return result == -1 ? ReadResult.Failed : ReadResult.Success;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool _Internal_Write(BytePayload payload, sbyte input)
        {
            payload.Stream.WriteByte((byte)input);
            return true;
        }

    }

    public static class SByteResolverExtensions
    {

        /// <summary>
        /// Writes a signed 8-bit integer to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void Write(this BytePayload payload, sbyte input)
            => SByteResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes a signed 8-bit integer array to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void Write(this BytePayload payload, sbyte[] input)
            => SByteResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes a signed 8-bit integer array to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void WriteArray(this BytePayload payload, sbyte[] input)
            => SByteResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes a signed 8-bit integer to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void WriteSByte(this BytePayload payload, sbyte input)
            => SByteResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes a signed 8-bit integer array to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void WriteSByteArray(this BytePayload payload, sbyte[] input)
            => SByteResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes a signed 8-bit integer to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void WriteInt8(this BytePayload payload, sbyte input)
            => SByteResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes a signed 8-bit integer array to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void WriteInt8Array(this BytePayload payload, sbyte[] input)
            => SByteResolver.Instance.Write(payload, input);

        /// <summary>
        /// Reads a signed 8-bit integer from the stream.
        /// </summary>
        public static sbyte ReadSByte(this BytePayload payload)
        {
            SByteResolver.Instance.Read(payload, out sbyte output);
            return output;
        }

        /// <summary>
        /// Reads a signed 8-bit integer array from the stream.
        /// </summary>
        public static sbyte[] ReadSByteArray(this BytePayload payload)
        {
            SByteResolver.Instance.Read(payload, out sbyte[] output);
            return output;
        }

        /// <summary>
        /// Reads a signed 8-bit integer from the stream.
        /// </summary>
        public static sbyte ReadInt8(this BytePayload payload)
        {
            SByteResolver.Instance.Read(payload, out sbyte output);
            return output;
        }

        /// <summary>
        /// Reads a signed 8-bit integer array from the stream.
        /// </summary>
        public static sbyte[] ReadInt8Array(this BytePayload payload)
        {
            SByteResolver.Instance.Read(payload, out sbyte[] output);
            return output;
        }
    }

}

