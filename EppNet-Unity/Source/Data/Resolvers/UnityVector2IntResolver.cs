///////////////////////////////////////////////////////
/// Filename: UnityVector2IntResolver.cs
/// Date: September 2, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using UnityEngine;

namespace EppNet.Data.Unity
{

    public class UnityVector2IntResolver : VectorResolverBase<Vector2Int>
    {

        public static readonly UnityVector2IntResolver Instance = new UnityVector2IntResolver();

        public UnityVector2IntResolver() : base(false)
        {
            this.Default = Vector2Int.zero;
            this.UnitX = Vector2Int.right;
            this.UnitY = Vector2Int.up;
            this.UnitZ = this.UnitW = this.Default;
            this.One = Vector2Int.one;
            this.NumComponents = 3;
        }

        public override Vector2Int Put(Vector2Int input, float value, int index)
        {
            input[index] = (int)value;
            return input;
        }

    }

    public static class UnityVector2IntResolverExtensions
    {

        /// <summary>
        /// Writes a <see cref="Vector2Int"/> to the stream.<br/><br/>
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
        /// The following <see cref="Vector2Int"/>s are considered special and cost 1 byte:<br/>
        /// - <see cref="Vector2Int.zero"/><br/>
        /// - <see cref="Vector2Int.right"/><br/>
        /// - <see cref="Vector2Int.up"/><br/>
        /// - <see cref="Vector2Int.forward"/><br/>
        /// - <see cref="Vector2Int.one"/><br/><br/>
        /// 
        /// Bandwidth cost: 1 byte to 13 bytes
        /// </summary>
        /// <param name="payload">The BytePayload to write data to.</param>
        /// <param name="input">The Vector2Int to transmit.</param>
        /// <param name="absolute">Should ABSOLUTELY all components be transmitted?</param>
        /// <returns>Whether or not the Vector2Int was written</returns>

        public static bool Write(this BytePayload payload, Vector2Int input, bool absolute = true)
            => UnityVector2IntResolver.Instance.Write(payload, input, absolute);

        /// <summary>
        /// Writes a <see cref="Vector2Int"/> array to the stream.<br/><br/>
        /// 
        /// Empty and null arrays cost 1 byte to write to the wire.<br/>
        /// See <see cref="Write(BytePayload, Vector2Int, bool)"/> for more information on 
        /// how each is written.<br/><br/>
        /// 
        /// Bandwidth cost: 1 byte to N * 13 bytes where N is the number of array elements
        /// </summary>
        /// <param name="payload">The BytePayload to write data to.</param>
        /// <param name="input">The Vector2Int to transmit.</param>
        /// <param name="absolute">Should ABSOLUTELY all components be transmitted?</param>
        /// <returns>Whether or not the Vector2Int array was written</returns>

        public static bool Write(this BytePayload payload, Vector2Int[] input, bool absolute = true)
            => UnityVector2IntResolver.Instance.WriteArray(payload, input, absolute);

        /// <summary>
        /// Writes a <see cref="Vector2Int"/> array to the stream.<br/><br/>
        /// 
        /// Empty and null arrays cost 1 byte to write to the wire.<br/>
        /// See <see cref="Write(BytePayload, Vector2Int, bool)"/> for more information on 
        /// how each is written.<br/><br/>
        /// 
        /// Bandwidth cost: 1 byte to N * 13 bytes where N is the number of array elements
        /// </summary>
        /// <param name="payload">The BytePayload to write data to.</param>
        /// <param name="input">The Vector2Int to transmit.</param>
        /// <param name="absolute">Should ABSOLUTELY all components be transmitted?</param>
        /// <returns>Whether or not the Vector2Int array was written</returns>

        public static bool WriteArray(this BytePayload payload, Vector2Int[] input, bool absolute = true)
            => UnityVector2IntResolver.Instance.WriteArray(payload, input, absolute);

        /// <summary>
        /// Writes a <see cref="Vector2Int"/> in <b>DELTA MODE</b> to the stream.<br/>
        /// Only non-zero components are transmitted.<br/><br/>
        ///
        /// <paramref name="newVector"/> - <paramref name="oldVector"/> = delta Vector2Int<br/><br/>
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
        /// The following <see cref="Vector2Int"/>s are considered special and cost 1 byte:<br/>
        /// - <see cref="Vector2Int.zero"/><br/>
        /// - <see cref="Vector2Int.right"/><br/>
        /// - <see cref="Vector2Int.up"/><br/>
        /// - <see cref="Vector2Int.forward"/><br/>
        /// - <see cref="Vector2Int.one"/><br/><br/>
        /// 
        /// Bandwidth cost: 1 byte to 13 bytes
        /// </summary>
        /// <param name="payload">The BytePayload to write data to.</param>
        /// <param name="newVector">The new vector</param>
        /// <param name="oldVector">The old vector</param>
        /// <returns>Whether or not the Vector2Int was written</returns>

        public static bool Write(this BytePayload payload, Vector2Int newVector, Vector2Int oldVector)
        {
            Vector2Int delta = newVector - oldVector;
            return UnityVector2IntResolver.Instance.Write(payload, delta, absolute: false);
        }

        /// <summary>
        /// Writes a <see cref="Vector2Int"/> to the stream.<br/><br/>
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
        /// The following <see cref="Vector2Int"/>s are considered special and cost 1 byte:<br/>
        /// - <see cref="Vector2Int.zero"/><br/>
        /// - <see cref="Vector2Int.right"/><br/>
        /// - <see cref="Vector2Int.up"/><br/>
        /// - <see cref="Vector2Int.forward"/><br/>
        /// - <see cref="Vector2Int.one"/><br/><br/>
        /// 
        /// Bandwidth cost: 1 byte to 13 bytes
        /// </summary>
        /// <param name="payload">The BytePayload to write data to.</param>
        /// <param name="input">The Vector2Int to transmit.</param>
        /// <param name="absolute">Should ABSOLUTELY all components be transmitted?</param>
        /// <returns>Whether or not the Vector2Int was written</returns>

        public static bool WriteVector2Int(this BytePayload payload, Vector2Int input, bool absolute = true)
            => UnityVector2IntResolver.Instance.Write(payload, input, absolute);

        /// <summary>
        /// Writes a <see cref="Vector2Int"/> in <b>DELTA MODE</b> to the stream.<br/>
        /// Only non-zero components are transmitted.<br/><br/>
        ///
        /// <paramref name="newVector"/> - <paramref name="oldVector"/> = delta Vector2Int<br/><br/>
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
        /// The following <see cref="Vector2Int"/>s are considered special and cost 1 byte:<br/>
        /// - <see cref="Vector2Int.zero"/><br/>
        /// - <see cref="Vector2Int.right"/><br/>
        /// - <see cref="Vector2Int.up"/><br/>
        /// - <see cref="Vector2Int.forward"/><br/>
        /// - <see cref="Vector2Int.one"/><br/><br/>
        /// 
        /// Bandwidth cost: 1 byte to 13 bytes
        /// </summary>
        /// <param name="payload">The BytePayload to write data to.</param>
        /// <param name="newVector">The new vector</param>
        /// <param name="oldVector">The old vector</param>
        /// <returns>Whether or not the Vector2Int was written</returns>

        public static bool WriteVector2Int(this BytePayload payload, Vector2Int newVector, Vector2Int oldVector)
        {
            Vector2Int delta = newVector - oldVector;
            return UnityVector2IntResolver.Instance.Write(payload, delta, absolute: false);
        }

        /// <summary>
        /// <b><i>This method is not recommended!</i></b><br/>
        /// Disregards if fetched vector is absolute or delta!<br/><br/>
        /// Only use this if you're sure the received vector is absolute.<br/><br/>
        /// 
        /// Reads an assumed to be absolute <see cref="Vector2Int"/> from the stream.
        /// </summary>
        /// <param name="payload">The BytePayload to read data from.</param>
        /// <returns></returns>

        public static Vector2Int ReadVector2Int(this BytePayload payload)
        {
            UnityVector2IntResolver.Instance.Read(payload, out Vector2Int result);
            return result;
        }

        /// <summary>
        /// Reads a <see cref="Vector2Int"/> array from the stream. Outputs if it's an absolute vector array,
        /// </summary>
        /// <param name="payload">The BytePayload to read data from.</param>
        /// <param name="isAbsolute">If the received array contains absolute vectors</param>
        /// <returns>The received Vector2Int array</returns>

        public static Vector2Int[] ReadVector2IntArray(this BytePayload payload, out bool isAbsolute)
        {
            ReadResult result = UnityVector2IntResolver.Instance.Read(payload, out Vector2Int[] output);
            isAbsolute = result == ReadResult.Success;
            return output;
        }

        /// <summary>
        /// <b><i>Recommended over <see cref="ReadVector2Int(BytePayload)"/></i></b><br/><br/>
        /// Reads a <see cref="Vector2Int"/> from the stream, and, if in delta mode, adds<br/>
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

        public static Vector2Int ReadVector2Int(this BytePayload payload, Vector2Int oldVector)
        {
            ReadResult result = UnityVector2IntResolver.Instance.Read(payload, out Vector2Int output);

            if (result == ReadResult.SuccessDelta)
                return oldVector + output;

            return output;
        }

        /// <summary>
        /// <b><i>Recommended over <see cref="ReadVector2Int(BytePayload)"/></i></b><br/><br/>
        /// Reads a <see cref="Vector2Int"/> from the stream. Outputs if it's an absolute vector.
        /// </summary>
        /// <param name="payload">The BytePayload to read data from.</param>
        /// <param name="isAbsolute">If the received vector is absolute</param>
        /// <returns>The received Vector2Int</returns>

        public static Vector2Int ReadVector2Int(this BytePayload payload, out bool isAbsolute)
        {
            ReadResult result = UnityVector2IntResolver.Instance.Read(payload, out Vector2Int output);
            isAbsolute = result == ReadResult.Success;
            return output;
        }

    }

}
