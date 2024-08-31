///////////////////////////////////////////////////////
/// Filename: Vector2Resolver.cs
/// Date: August 30, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;
using System.Numerics;

namespace EppNet.Data
{

    public class Vector2Resolver : VectorResolverBase<Vector2>
    {

        public static readonly Vector2Resolver Instance = new();

        public Vector2Resolver() : base(autoAdvance: false)
        {
            this.Default = Vector2.Zero;
            this.UnitX = Vector2.UnitX;
            this.UnitY = Vector2.UnitY;
            this.UnitZ = this.UnitW = Vector2.Zero;
            this.One = Vector2.One;
            this.NumComponents = 2;
        }

        public override Span<float> GetFloats(Vector2 input, scoped ref Span<float> floats)
        {
            floats[0] = input.X;
            floats[1] = input.Y;
            return floats;
        }

        public override Vector2 Put(Vector2 input, float value, int index)
        {
            input[index] = value;
            return input;
        }
    }

    public static class Vector2ResolverExtensions
    {

        /// <summary>
        /// Writes a <see cref="Vector2"/> to the stream.<br/><br/>
        /// 
        /// <b><i>Transmission Modes</i></b><br/>
        /// <paramref name="absolute"/>: Absolutely every component will be sent over the wire.<br/>
        /// !<paramref name="absolute"/>/delta: Only non-zero components will be sent over the wire.<br/><br/>
        /// 
        /// <b><i>Component Transmission</i></b><br/>
        /// Components can be sent as <see cref="sbyte"/>, <see cref="short"/>,
        /// <see cref="int"/>, or a <see cref="float"/> with lossy precision.<br/><br/>
        /// 
        /// Components are sent as the smallest possible type that can contain each
        /// component.<br/>
        /// 
        /// <b>Float Precision</b> is dictated by <see cref="BytePayload.FloatPrecision"/>.
        /// 
        /// <br/><br/>
        /// The following <see cref="Vector2"/>s are considered special and cost 1 byte:<br/>
        /// - <see cref="Vector2.Zero"/><br/>
        /// - <see cref="Vector2.UnitX"/><br/>
        /// - <see cref="Vector2.UnitY"/><br/>
        /// - <see cref="Vector2.One"/><br/><br/>
        /// 
        /// Bandwidth cost: 1 byte to 9 bytes
        /// </summary>
        /// <param name="payload">The BytePayload to write data to.</param>
        /// <param name="input">The Vector2 to transmit.</param>
        /// <param name="absolute">Should ABSOLUTELY all components be transmitted?</param>
        /// <returns>Whether or not the Vector2 was written</returns>

        public static bool Write(this BytePayload payload, Vector2 input, bool absolute = true)
            => Vector2Resolver.Instance.Write(payload, input, absolute);

        /// <summary>
        /// Writes a <see cref="Vector2"/> array to the stream.<br/><br/>
        /// 
        /// Empty and null arrays cost 1 byte to write to the wire.<br/>
        /// See <see cref="Write(BytePayload, Vector2, bool)"/> for more information on 
        /// how each is written.<br/><br/>
        /// 
        /// Bandwidth cost: 1 byte to N * 9 bytes where N is the number of array elements
        /// </summary>
        /// <param name="payload">The BytePayload to write data to.</param>
        /// <param name="input">The Vector2 to transmit.</param>
        /// <param name="absolute">Should ABSOLUTELY all components be transmitted?</param>
        /// <returns>Whether or not the Vector2 array was written</returns>

        public static bool Write(this BytePayload payload, Vector2[] input, bool absolute = true)
            => Vector2Resolver.Instance.WriteArray(payload, input, absolute);

        /// <summary>
        /// Writes a <see cref="Vector2"/> array to the stream.<br/><br/>
        /// 
        /// Empty and null arrays cost 1 byte to write to the wire.<br/>
        /// See <see cref="Write(BytePayload, Vector2, bool)"/> for more information on 
        /// how each is written.<br/><br/>
        /// 
        /// Bandwidth cost: 1 byte to N * 9 bytes where N is the number of array elements
        /// </summary>
        /// <param name="payload">The BytePayload to write data to.</param>
        /// <param name="input">The Vector2 to transmit.</param>
        /// <param name="absolute">Should ABSOLUTELY all components be transmitted?</param>
        /// <returns>Whether or not the Vector2 array was written</returns>

        public static bool WriteArray(this BytePayload payload, Vector2[] input, bool absolute = true)
            => Vector2Resolver.Instance.WriteArray(payload, input, absolute);

        /// <summary>
        /// Writes a <see cref="Vector2"/> in <b>DELTA MODE</b> to the stream.<br/>
        /// Only non-zero components are transmitted.<br/><br/>
        ///
        /// <paramref name="newVector"/> - <paramref name="oldVector"/> = delta Vector2<br/><br/>
        /// 
        /// <b><i>Component Transmission</i></b><br/>
        /// Components can be sent as <see cref="sbyte"/>, <see cref="short"/>,
        /// <see cref="int"/>, or a <see cref="float"/> with lossy precision.<br/><br/>
        /// 
        /// Components are sent as the smallest possible type that can contain each
        /// component.<br/>
        /// 
        /// <b>Float Precision</b> is dictated by <see cref="BytePayload.FloatPrecision"/>.
        /// 
        /// <br/><br/>
        /// The following <see cref="Vector2"/>s are considered special and cost 1 byte:<br/>
        /// - <see cref="Vector2.Zero"/><br/>
        /// - <see cref="Vector2.UnitX"/><br/>
        /// - <see cref="Vector2.UnitY"/><br/>
        /// - <see cref="Vector2.One"/><br/><br/>
        /// 
        /// Bandwidth cost: 1 byte to 9 bytes
        /// </summary>
        /// <param name="payload">The BytePayload to write data to.</param>
        /// <param name="newVector">The new vector</param>
        /// <param name="oldVector">The old vector</param>
        /// <returns>Whether or not the Vector2 was written</returns>

        public static bool Write(this BytePayload payload, Vector2 newVector, Vector2 oldVector)
        {
            Vector2 delta = newVector - oldVector;
            return Vector2Resolver.Instance.Write(payload, delta, absolute: false);
        }

        /// <summary>
        /// Writes a <see cref="Vector2"/> to the stream.<br/><br/>
        /// 
        /// <b><i>Transmission Modes</i></b><br/>
        /// <paramref name="absolute"/>: Absolutely every component will be sent over the wire.<br/>
        /// !<paramref name="absolute"/>/delta: Only non-zero components will be sent over the wire.<br/><br/>
        /// 
        /// <b><i>Component Transmission</i></b><br/>
        /// Components can be sent as <see cref="sbyte"/>, <see cref="short"/>,
        /// <see cref="int"/>, or a <see cref="float"/> with lossy precision.<br/><br/>
        /// 
        /// Components are sent as the smallest possible type that can contain each
        /// component.<br/>
        /// 
        /// <b>Float Precision</b> is dictated by <see cref="BytePayload.FloatPrecision"/>.
        /// 
        /// <br/><br/>
        /// The following <see cref="Vector2"/>s are considered special and cost 1 byte:<br/>
        /// - <see cref="Vector2.Zero"/><br/>
        /// - <see cref="Vector2.UnitX"/><br/>
        /// - <see cref="Vector2.UnitY"/><br/>
        /// - <see cref="Vector2.One"/><br/><br/>
        /// 
        /// Bandwidth cost: 1 byte to 9 bytes
        /// </summary>
        /// <param name="payload">The BytePayload to write data to.</param>
        /// <param name="input">The Vector2 to transmit.</param>
        /// <param name="absolute">Should ABSOLUTELY all components be transmitted?</param>
        /// <returns>Whether or not the Vector2 was written</returns>

        public static bool WriteVector2(this BytePayload payload, Vector2 input, bool absolute = true)
            => Vector2Resolver.Instance.Write(payload, input, absolute);

        /// <summary>
        /// Writes a <see cref="Vector2"/> in <b>DELTA MODE</b> to the stream.<br/>
        /// Only non-zero components are transmitted.<br/><br/>
        ///
        /// <paramref name="newVector"/> - <paramref name="oldVector"/> = delta Vector2<br/><br/>
        /// 
        /// <b><i>Component Transmission</i></b><br/>
        /// Components can be sent as <see cref="sbyte"/>, <see cref="short"/>,
        /// <see cref="int"/>, or a <see cref="float"/> with lossy precision.<br/><br/>
        /// 
        /// Components are sent as the smallest possible type that can contain each
        /// component.<br/>
        /// 
        /// <b>Float Precision</b> is dictated by <see cref="BytePayload.FloatPrecision"/>.
        /// 
        /// <br/><br/>
        /// The following <see cref="Vector2"/>s are considered special and cost 1 byte:<br/>
        /// - <see cref="Vector2.Zero"/><br/>
        /// - <see cref="Vector2.UnitX"/><br/>
        /// - <see cref="Vector2.UnitY"/><br/>
        /// - <see cref="Vector2.One"/><br/><br/>
        /// 
        /// Bandwidth cost: 1 byte to 9 bytes
        /// </summary>
        /// <param name="payload">The BytePayload to write data to.</param>
        /// <param name="newVector">The new vector</param>
        /// <param name="oldVector">The old vector</param>
        /// <returns>Whether or not the Vector2 was written</returns>

        public static bool WriteVector2(this BytePayload payload, Vector2 newVector, Vector2 oldVector)
        {
            Vector2 delta = newVector - oldVector;
            return Vector2Resolver.Instance.Write(payload, delta, absolute: false);
        }

        /// <summary>
        /// <b><i>This method is not recommended!</i></b><br/>
        /// Disregards if fetched vector is absolute or delta!<br/><br/>
        /// Only use this if you're sure the received vector is absolute.<br/><br/>
        /// 
        /// Reads an assumed to be absolute <see cref="Vector2"/> from the stream.
        /// </summary>
        /// <param name="payload">The BytePayload to read data from.</param>
        /// <returns></returns>

        public static Vector2 ReadVector2(this BytePayload payload)
        {
            Vector2Resolver.Instance.Read(payload, out Vector2 result);
            return result;
        }

        /// <summary>
        /// Reads a <see cref="Vector2"/> array from the stream. Outputs if it's an absolute vector array,
        /// </summary>
        /// <param name="payload">The BytePayload to read data from.</param>
        /// <param name="isAbsolute">If the received array contains absolute vectors</param>
        /// <returns>The received Vector2 array</returns>

        public static Vector2[] ReadVector2Array(this BytePayload payload, out bool isAbsolute)
        {
            ReadResult result = Vector2Resolver.Instance.Read(payload, out Vector2[] output);
            isAbsolute = result == ReadResult.Success;
            return output;
        }

        /// <summary>
        /// <b><i>Recommended over <see cref="ReadVector2(BytePayload)"/></i></b><br/><br/>
        /// Reads a <see cref="Vector2"/> from the stream, and, if in delta mode, adds<br/>
        /// <paramref name="oldVector"/> to the result.
        /// </summary>
        /// <param name="payload">The BytePayload to read data from.</param>
        /// <param name="oldVector">The vector to add if the received one is a delta vector</param>
        /// <returns>
        /// <b>Absolute Mode</b>
        /// <br/>Received vector<br/><br/>
        /// <b>Delta Mode</b>
        /// <br/>Received vector + <paramref name="oldVector"/>
        /// </returns>

        public static Vector2 ReadVector2(this BytePayload payload, Vector2 oldVector)
        {
            ReadResult result = Vector2Resolver.Instance.Read(payload, out Vector2 output);

            if (result == ReadResult.SuccessDelta)
                return oldVector + output;

            return output;
        }

        /// <summary>
        /// <b><i>Recommended over <see cref="ReadVector2(BytePayload)"/></i></b><br/><br/>
        /// Reads a <see cref="Vector2"/> from the stream. Outputs if it's an absolute vector.
        /// </summary>
        /// <param name="payload">The BytePayload to read data from.</param>
        /// <param name="isAbsolute">If the received vector is absolute</param>
        /// <returns>The received Vector2</returns>

        public static Vector2 ReadVector2(this BytePayload payload, out bool isAbsolute)
        {
            ReadResult result = Vector2Resolver.Instance.Read(payload, out Vector2 output);
            isAbsolute = result == ReadResult.Success;
            return output;
        }

    }

}
