#if EPPNET_UNITY
///////////////////////////////////////////////////////
/// Filename: UnityVector3IntResolver.cs
/// Date: September 2, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using UnityEngine;

namespace EppNet.Data.Unity
{

    public class UnityVector3IntResolver : VectorResolverBase<Vector3Int>
    {

        public static readonly UnityVector3IntResolver Instance = new UnityVector3IntResolver();

        public UnityVector3IntResolver() : base(false)
        {
            this.Default = Vector3Int.zero;
            this.UnitX = Vector3Int.right;
            this.UnitY = Vector3Int.up;
            this.UnitZ = Vector3Int.forward;
            this.UnitW = this.Default;
            this.One = Vector3Int.one;
            this.NumComponents = 3;
        }

        public override Vector3Int Put(Vector3Int input, float value, int index)
        {
            input[index] = (int)value;
            return input;
        }

    }

    public static class UnityVector3IntResolverExtensions
    {

        /// <summary>
        /// Writes a <see cref="Vector3Int"/> to the stream.<br/><br/>
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
        /// The following <see cref="Vector3Int"/>s are considered special and cost 1 byte:<br/>
        /// - <see cref="Vector3Int.zero"/><br/>
        /// - <see cref="Vector3Int.right"/><br/>
        /// - <see cref="Vector3Int.up"/><br/>
        /// - <see cref="Vector3Int.forward"/><br/>
        /// - <see cref="Vector3Int.one"/><br/><br/>
        /// 
        /// Bandwidth cost: 1 byte to 13 bytes
        /// </summary>
        /// <param name="payload">The BytePayload to write data to.</param>
        /// <param name="input">The Vector3Int to transmit.</param>
        /// <param name="absolute">Should ABSOLUTELY all components be transmitted?</param>
        /// <returns>Whether or not the Vector3Int was written</returns>

        public static bool Write(this BytePayload payload, Vector3Int input, bool absolute = true)
            => UnityVector3IntResolver.Instance.Write(payload, input, absolute);

        /// <summary>
        /// Writes a <see cref="Vector3Int"/> array to the stream.<br/><br/>
        /// 
        /// Empty and null arrays cost 1 byte to write to the wire.<br/>
        /// See <see cref="Write(BytePayload, Vector3Int, bool)"/> for more information on 
        /// how each is written.<br/><br/>
        /// 
        /// Bandwidth cost: 1 byte to N * 13 bytes where N is the number of array elements
        /// </summary>
        /// <param name="payload">The BytePayload to write data to.</param>
        /// <param name="input">The Vector3Int to transmit.</param>
        /// <param name="absolute">Should ABSOLUTELY all components be transmitted?</param>
        /// <returns>Whether or not the Vector3Int array was written</returns>

        public static bool Write(this BytePayload payload, Vector3Int[] input, bool absolute = true)
            => UnityVector3IntResolver.Instance.WriteArray(payload, input, absolute);

        /// <summary>
        /// Writes a <see cref="Vector3Int"/> array to the stream.<br/><br/>
        /// 
        /// Empty and null arrays cost 1 byte to write to the wire.<br/>
        /// See <see cref="Write(BytePayload, Vector3Int, bool)"/> for more information on 
        /// how each is written.<br/><br/>
        /// 
        /// Bandwidth cost: 1 byte to N * 13 bytes where N is the number of array elements
        /// </summary>
        /// <param name="payload">The BytePayload to write data to.</param>
        /// <param name="input">The Vector3Int to transmit.</param>
        /// <param name="absolute">Should ABSOLUTELY all components be transmitted?</param>
        /// <returns>Whether or not the Vector3Int array was written</returns>

        public static bool WriteArray(this BytePayload payload, Vector3Int[] input, bool absolute = true)
            => UnityVector3IntResolver.Instance.WriteArray(payload, input, absolute);

        /// <summary>
        /// Writes a <see cref="Vector3Int"/> in <b>DELTA MODE</b> to the stream.<br/>
        /// Only non-zero components are transmitted.<br/><br/>
        ///
        /// <paramref name="newVector"/> - <paramref name="oldVector"/> = delta Vector3Int<br/><br/>
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
        /// The following <see cref="Vector3Int"/>s are considered special and cost 1 byte:<br/>
        /// - <see cref="Vector3Int.zero"/><br/>
        /// - <see cref="Vector3Int.right"/><br/>
        /// - <see cref="Vector3Int.up"/><br/>
        /// - <see cref="Vector3Int.forward"/><br/>
        /// - <see cref="Vector3Int.one"/><br/><br/>
        /// 
        /// Bandwidth cost: 1 byte to 13 bytes
        /// </summary>
        /// <param name="payload">The BytePayload to write data to.</param>
        /// <param name="newVector">The new vector</param>
        /// <param name="oldVector">The old vector</param>
        /// <returns>Whether or not the Vector3Int was written</returns>

        public static bool Write(this BytePayload payload, Vector3Int newVector, Vector3Int oldVector)
        {
            Vector3Int delta = newVector - oldVector;
            return UnityVector3IntResolver.Instance.Write(payload, delta, absolute: false);
        }

        /// <summary>
        /// Writes a <see cref="Vector3Int"/> to the stream.<br/><br/>
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
        /// The following <see cref="Vector3Int"/>s are considered special and cost 1 byte:<br/>
        /// - <see cref="Vector3Int.zero"/><br/>
        /// - <see cref="Vector3Int.right"/><br/>
        /// - <see cref="Vector3Int.up"/><br/>
        /// - <see cref="Vector3Int.forward"/><br/>
        /// - <see cref="Vector3Int.one"/><br/><br/>
        /// 
        /// Bandwidth cost: 1 byte to 13 bytes
        /// </summary>
        /// <param name="payload">The BytePayload to write data to.</param>
        /// <param name="input">The Vector3Int to transmit.</param>
        /// <param name="absolute">Should ABSOLUTELY all components be transmitted?</param>
        /// <returns>Whether or not the Vector3Int was written</returns>

        public static bool WriteVector3Int(this BytePayload payload, Vector3Int input, bool absolute = true)
            => UnityVector3IntResolver.Instance.Write(payload, input, absolute);

        /// <summary>
        /// Writes a <see cref="Vector3Int"/> in <b>DELTA MODE</b> to the stream.<br/>
        /// Only non-zero components are transmitted.<br/><br/>
        ///
        /// <paramref name="newVector"/> - <paramref name="oldVector"/> = delta Vector3Int<br/><br/>
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
        /// The following <see cref="Vector3Int"/>s are considered special and cost 1 byte:<br/>
        /// - <see cref="Vector3Int.zero"/><br/>
        /// - <see cref="Vector3Int.right"/><br/>
        /// - <see cref="Vector3Int.up"/><br/>
        /// - <see cref="Vector3Int.forward"/><br/>
        /// - <see cref="Vector3Int.one"/><br/><br/>
        /// 
        /// Bandwidth cost: 1 byte to 13 bytes
        /// </summary>
        /// <param name="payload">The BytePayload to write data to.</param>
        /// <param name="newVector">The new vector</param>
        /// <param name="oldVector">The old vector</param>
        /// <returns>Whether or not the Vector3Int was written</returns>

        public static bool WriteVector3Int(this BytePayload payload, Vector3Int newVector, Vector3Int oldVector)
        {
            Vector3Int delta = newVector - oldVector;
            return UnityVector3IntResolver.Instance.Write(payload, delta, absolute: false);
        }

        /// <summary>
        /// <b><i>This method is not recommended!</i></b><br/>
        /// Disregards if fetched vector is absolute or delta!<br/><br/>
        /// Only use this if you're sure the received vector is absolute.<br/><br/>
        /// 
        /// Reads an assumed to be absolute <see cref="Vector3Int"/> from the stream.
        /// </summary>
        /// <param name="payload">The BytePayload to read data from.</param>
        /// <returns></returns>

        public static Vector3Int ReadVector3Int(this BytePayload payload)
        {
            UnityVector3IntResolver.Instance.Read(payload, out Vector3Int result);
            return result;
        }

        /// <summary>
        /// Reads a <see cref="Vector3Int"/> array from the stream. Outputs if it's an absolute vector array,
        /// </summary>
        /// <param name="payload">The BytePayload to read data from.</param>
        /// <param name="isAbsolute">If the received array contains absolute vectors</param>
        /// <returns>The received Vector3Int array</returns>

        public static Vector3Int[] ReadVector3IntArray(this BytePayload payload, out bool isAbsolute)
        {
            ReadResult result = UnityVector3IntResolver.Instance.Read(payload, out Vector3Int[] output);
            isAbsolute = result == ReadResult.Success;
            return output;
        }

        /// <summary>
        /// <b><i>Recommended over <see cref="ReadVector3Int(BytePayload)"/></i></b><br/><br/>
        /// Reads a <see cref="Vector3Int"/> from the stream, and, if in delta mode, adds<br/>
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

        public static Vector3Int ReadVector3Int(this BytePayload payload, Vector3Int oldVector)
        {
            ReadResult result = UnityVector3IntResolver.Instance.Read(payload, out Vector3Int output);

            if (result == ReadResult.SuccessDelta)
                return oldVector + output;

            return output;
        }

        /// <summary>
        /// <b><i>Recommended over <see cref="ReadVector3Int(BytePayload)"/></i></b><br/><br/>
        /// Reads a <see cref="Vector3Int"/> from the stream. Outputs if it's an absolute vector.
        /// </summary>
        /// <param name="payload">The BytePayload to read data from.</param>
        /// <param name="isAbsolute">If the received vector is absolute</param>
        /// <returns>The received Vector3Int</returns>

        public static Vector3Int ReadVector3Int(this BytePayload payload, out bool isAbsolute)
        {
            ReadResult result = UnityVector3IntResolver.Instance.Read(payload, out Vector3Int output);
            isAbsolute = result == ReadResult.Success;
            return output;
        }

    }

}
#endif