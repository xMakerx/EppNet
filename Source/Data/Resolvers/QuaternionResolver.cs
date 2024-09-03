﻿///////////////////////////////////////////////////////
/// Filename: QuaternionResolver.cs
/// Date: August 29, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System.Numerics;

namespace EppNet.Data
{

    public class QuaternionResolver : QuaternionResolverBase<Quaternion>
    {

        public static readonly QuaternionResolver Instance = new();


        public QuaternionResolver()
        {
            this.Zero = new(0, 0, 0, 0);
            this.Identity = Quaternion.Identity;
        }

        public override Quaternion FromAdapter(QuaternionAdapter adapter) => adapter;

        public override QuaternionAdapter ToAdapter(Quaternion input) => (QuaternionAdapter) input;
    }

    public static class QuaternionResolverExtensions
    {

        /// <summary>
        /// Writes a normalized <see cref="Quaternion"/> to the stream using the Smallest Three algorithm.<br/>
        /// Cost varies based on if <see cref="QuaternionResolver.ByteQuantization"/> is enabled, and the
        /// particular quaternion sent. <see cref="Quaternion.Zero"/> and <see cref="Quaternion.Identity"/> are
        /// the cheapest at 1 byte each. <br/><br/>
        /// With quantization enabled, the maximum size for a quaternion is 4 bytes; otherwise, it sends
        /// <br/>one single per value which is 4 bytes per value or 33 bytes including the header.<br/><br/>
        /// Effective margin of error: ±0.0156
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="input"></param>
        public static void Write(this BytePayload payload, Quaternion input)
            => QuaternionResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes an array of <see cref="Quaternion"/> to the stream<br/>
        /// See <see cref="Write(BytePayload, Quaternion)"/> for more information on how each quaternion is written
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="input"></param>
        public static void Write(this BytePayload payload, Quaternion[] input)
            => QuaternionResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes an array of <see cref="Quaternion"/> to the stream<br/>
        /// See <see cref="Write(BytePayload, Quaternion)"/> for more information on how each quaternion is written
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="input"></param>
        public static void WriteArray(this BytePayload payload, Quaternion[] input)
            => QuaternionResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes a normalized <see cref="Quaternion"/> to the stream using the Smallest Three algorithm.<br/>
        /// Cost varies based on if <see cref="QuaternionResolver.ByteQuantization"/> is enabled, and the
        /// particular quaternion sent. <see cref="Quaternion.Zero"/> and <see cref="Quaternion.Identity"/> are
        /// the cheapest at 1 byte each. <br/><br/>
        /// With quantization enabled, the maximum size for a quaternion is 4 bytes; otherwise, it sends
        /// <br/>one single per value which is 4 bytes per value or 33 bytes including the header.<br/><br/>
        /// Effective margin of error: ±0.0156
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="input"></param>
        public static void WriteQuat(this BytePayload payload, Quaternion input)
            => QuaternionResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes an array of <see cref="Quaternion"/> to the stream<br/>
        /// See <see cref="Write(BytePayload, Quaternion)"/> for more information on how each quaternion is written
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="input"></param>
        public static void WriteQuatArray(this BytePayload payload, Quaternion[] input)
            => QuaternionResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes a normalized <see cref="Quaternion"/> to the stream using the Smallest Three algorithm.<br/>
        /// Cost varies based on if <see cref="QuaternionResolver.ByteQuantization"/> is enabled, and the
        /// particular quaternion sent. <see cref="Quaternion.Zero"/> and <see cref="Quaternion.Identity"/> are
        /// the cheapest at 1 byte each. <br/><br/>
        /// With quantization enabled, the maximum size for a quaternion is 4 bytes; otherwise, it sends
        /// <br/>one single per value which is 4 bytes per value or 33 bytes including the header.<br/><br/>
        /// Effective margin of error: ±0.0156
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="input"></param>
        public static void WriteQuaternion(this BytePayload payload, Quaternion input)
            => QuaternionResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes an array of <see cref="Quaternion"/> to the stream<br/>
        /// See <see cref="Write(BytePayload, Quaternion)"/> for more information on how each quaternion is written
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="input"></param>
        public static void WriteQuaternionArray(this BytePayload payload, Quaternion[] input)
            => QuaternionResolver.Instance.Write(payload, input);


        /// <summary>
        /// Reads a normalized <see cref="Quaternion"/> from the stream.<br/>
        /// For more information on how these are encoded, see: <see cref="Write(BytePayload, Quaternion)"/><br/>
        /// Effective margin of error: ±0.0156
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>

        public static Quaternion ReadQuat(this BytePayload payload)
        {
            QuaternionResolver.Instance.Read(payload, out Quaternion output);
            return output;
        }

        /// <summary>
        /// Reads a normalized <see cref="Quaternion"/> array from the stream.<br/>
        /// For more information on how these are encoded, see: <see cref="Write(BytePayload, Quaternion)"/><br/>
        /// Effective margin of error: ±0.0156
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>

        public static Quaternion[] ReadQuatArray(this BytePayload payload)
        {
            QuaternionResolver.Instance.Read(payload, out Quaternion[] output);
            return output;
        }

        /// <summary>
        /// Reads a normalized <see cref="Quaternion"/> from the stream.<br/>
        /// For more information on how these are encoded, see: <see cref="Write(BytePayload, Quaternion)"/><br/>
        /// Effective margin of error: ±0.0156
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>

        public static Quaternion ReadQuaternion(this BytePayload payload)
        {
            QuaternionResolver.Instance.Read(payload, out Quaternion output);
            return output;
        }

        /// <summary>
        /// Reads a normalized <see cref="Quaternion"/> array from the stream.<br/>
        /// For more information on how these are encoded, see: <see cref="Write(BytePayload, Quaternion)"/><br/>
        /// Effective margin of error: ±0.0156
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>

        public static Quaternion[] ReadQuaternionArray(this BytePayload payload)
        {
            QuaternionResolver.Instance.Read(payload, out Quaternion[] output);
            return output;
        }

    }

}