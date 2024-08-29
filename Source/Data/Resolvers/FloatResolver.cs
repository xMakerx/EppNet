///////////////////////////////////////////////////////
/// Filename: FloatResolver.cs
/// Date: August 28, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Utilities;

using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace EppNet.Data
{

    public class FloatResolver : Resolver<float>
    {

        public static readonly FloatResolver Instance = new();

        static FloatResolver()
            => BytePayload.AddResolver(typeof(float), Instance);

        public FloatResolver() : base(sizeof(int)) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool _Internal_Read(BytePayload payload, out float output)
        {
            Span<byte> buffer = stackalloc byte[Size];
            int read = payload.Stream.Read(buffer);
            
            bool didRead = BinaryPrimitives.TryReadInt32LittleEndian(buffer, out int i32);
            output = didRead ? (float)(i32 * BytePayload.PrecisionReturnDecimalPlaces) : float.NaN;
            return didRead;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool _Internal_Write(BytePayload payload, float input)
        {
            double rounded = input.Round(BytePayload.FloatPrecision);
            int i32 = (int)(rounded * BytePayload.PrecisionDecimalPlaces);
            return BinaryPrimitives.TryWriteInt32LittleEndian(payload.Stream.GetSpan(Size), i32);
        }

    }

    public static class FloatResolverExtensions
    {

        /// <summary>
        /// Writes a provided float input as a 32-bit integer.<br/>
        /// Floats are sent with the precision specified by <see cref="BytePayload.FloatPrecision"/>
        /// </summary>
        /// <param name="input"></param>

        public static void Write(this BytePayload payload, float input)
            => FloatResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes the provided float array as a signed 32-bit integer array to the stream.
        /// Floats are sent with the precision specified by <see cref="BytePayload.FloatPrecision"/>
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="input"></param>
        public static void Write(this BytePayload payload, float[] input)
            => FloatResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes the provided float array as a signed 32-bit integer array to the stream.
        /// Floats are sent with the precision specified by <see cref="BytePayload.FloatPrecision"/>
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="input"></param>
        public static void WriteArray(this BytePayload payload, float[] input)
            => FloatResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes a provided float input as a 32-bit integer.<br/>
        /// Floats are sent with the precision specified by <see cref="BytePayload.FloatPrecision"/>
        /// </summary>
        /// <param name="input"></param>

        public static void WriteSingle(this BytePayload payload, float input)
            => FloatResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes the provided float array as a signed 32-bit integer array to the stream.
        /// Floats are sent with the precision specified by <see cref="BytePayload.FloatPrecision"/>
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="input"></param>
        public static void WriteSingleArray(this BytePayload payload, float[] input)
            => FloatResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes a provided float input as a 32-bit integer.<br/>
        /// Floats are sent with the precision specified by <see cref="BytePayload.FloatPrecision"/>
        /// </summary>
        /// <param name="input"></param>

        public static void WriteFloat(this BytePayload payload, float input)
            => FloatResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes the provided float array as a signed 32-bit integer array to the stream.
        /// Floats are sent with the precision specified by <see cref="BytePayload.FloatPrecision"/>
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="input"></param>
        public static void WriteFloatArray(this BytePayload payload, float[] input)
            => FloatResolver.Instance.Write(payload, input);

        /// <summary>
        /// Reads a float from the stream.<br/>
        /// Floats are sent with the precision specified by <see cref="BytePayload.FloatPrecision"/>
        /// </summary>
        /// <returns><see cref="float.NaN"/> is returned if reading failed</returns>

        public static float ReadSingle(this BytePayload payload)
        {
            FloatResolver.Instance.Read(payload, out float output);
            return output;
        }

        /// <summary>
        /// Reads a signed 32-bit integer array from the stream and converts each element to the
        /// proper float. <br/>
        /// Floats are sent with the precision specified by <see cref="BytePayload.FloatPrecision"/>
        /// </summary>
        /// <returns><see cref="float.NaN"/> is provided for each element that failed to be read</returns>
        public static float[] ReadSingleArray(this BytePayload payload)
        {
            FloatResolver.Instance.Read(payload, out float[] output);
            return output;
        }

        /// <summary>
        /// Reads a float from the stream.<br/>
        /// Floats are sent with the precision specified by <see cref="BytePayload.FloatPrecision"/>
        /// </summary>
        /// <returns><see cref="float.NaN"/> is returned if reading failed</returns>

        public static float ReadFloat(this BytePayload payload)
        {
            FloatResolver.Instance.Read(payload, out float output);
            return output;
        }

        /// <summary>
        /// Reads a signed 32-bit integer array from the stream and converts each element to the
        /// proper float. <br/>
        /// Floats are sent with the precision specified by <see cref="BytePayload.FloatPrecision"/>
        /// </summary>
        /// <returns><see cref="float.NaN"/> is provided for each element that failed to be read</returns>
        public static float[] ReadFloatArray(this BytePayload payload)
        {
            FloatResolver.Instance.Read(payload, out float[] output);
            return output;
        }

    }

}
