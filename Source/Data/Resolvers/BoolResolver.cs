///////////////////////////////////////////////////////
/// Filename: BoolResolver.cs
/// Date: August 28, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace EppNet.Data
{

    public class BoolResolver : Resolver<bool>
    {

        public static BoolResolver Instance = new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override ReadResult _Internal_Read(BytePayload payload, out bool output)
        {
            int result = payload.Stream.ReadByte();
            output = ((byte)result == 1);

            return result == -1 ? ReadResult.Failed : ReadResult.Success;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool _Internal_Write(BytePayload payload, bool input)
        {
            payload.Stream.WriteByte(input ? (byte) 1 : (byte) 0);
            return true;
        }
    }

    public static class BoolResolverExtensions
    {

        /// <summary>
        /// Writes an unsigned 8-bit integer to the stream denoting 1 (true) or 0 (false).
        /// </summary>
        /// <param name="input"></param>
        public static void Write(this BytePayload payload, bool input)
            => BoolResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes an unsigned 8-bit integer array to the stream denoting 1 (true) or 0 (false).
        /// </summary>
        /// <param name="input"></param>
        public static void Write(this BytePayload payload, bool[] input)
            => BoolResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes an unsigned 8-bit integer array to the stream denoting 1 (true) or 0 (false).
        /// </summary>
        /// <param name="input"></param>
        public static void Write<TCollection>(this BytePayload payload, TCollection input) where TCollection : ICollection<bool>
            => BoolResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes an unsigned 8-bit integer array to the stream denoting 1 (true) or 0 (false).
        /// </summary>
        /// <param name="input"></param>
        public static void WriteArray(this BytePayload payload, bool[] input)
            => BoolResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes an unsigned 8-bit integer to the stream denoting 1 (true) or 0 (false).
        /// </summary>
        /// <param name="input"></param>
        public static void WriteBool(this BytePayload payload, bool input)
            => BoolResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes an unsigned 8-bit integer array to the stream denoting 1 (true) or 0 (false).
        /// </summary>
        /// <param name="input"></param>
        public static void WriteBoolArray(this BytePayload payload, bool[] input)
            => BoolResolver.Instance.Write(payload, input);

        /// <summary>
        /// Reads an unsigned 8-bit integer collection from the stream denoting 1 (true) or 0 (false).
        /// </summary>
        /// <param name="input"></param>
        public static TCollection Read<TCollection>(this BytePayload payload) where TCollection : class, ICollection<bool>, new()
        {
            BoolResolver.Instance.Read(payload, out TCollection output);
            return output;
        }

        /// <summary>
        /// Reads an unsigned 8-bit integer from the stream denoting 1 (true) or 0 (false).
        /// </summary>
        /// <param name="input"></param>
        public static bool ReadBool(this BytePayload payload)
        {
            BoolResolver.Instance.Read(payload, out bool output);
            return output;
        }

        /// <summary>
        /// Reads an unsigned 8-bit integer array from the stream denoting 1 (true) or 0 (false).
        /// </summary>
        /// <param name="input"></param>
        public static bool[] ReadBoolArray(this BytePayload payload)
        {
            BoolResolver.Instance.Read(payload, out bool[] output);
            return output;
        }


    }

}
