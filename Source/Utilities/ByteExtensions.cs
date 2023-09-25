///////////////////////////////////////////////////////
/// Filename: ByteExtensions.cs
/// Date: September 25, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;

namespace EppNet.Utilities
{

    public static class ByteExtensions
    {

        /// <summary>
        /// Checks if the bit at the specified index is on.
        /// </summary>
        /// <param name="b"></param>
        /// <param name="bitIndex"></param>
        /// <returns></returns>
        public static bool IsBitOn(this byte b, int bitIndex)
        {
            if (-1 < bitIndex && bitIndex < 8)
                return (b & (1 << bitIndex)) != 0;

            _IndexWarning("IsBitOn", bitIndex);
            return false;
        }

        /// <summary>
        /// Sets the bit at the specified index to 1.
        /// </summary>
        /// <param name="b"></param>
        /// <param name="bitIndex"></param>
        /// <returns>The updated byte or the same byte if index was out of range. </returns>

        public static byte EnableBit(this byte b, int bitIndex)
        {
            if (-1 < bitIndex && bitIndex < 8)
                b = (byte) (b | (1 << bitIndex));
            else
                _IndexWarning("EnableBit", bitIndex);

            return b;
        }

        /// <summary>
        /// Sets the bit at the specified index to 0.
        /// </summary>
        /// <param name="b"></param>
        /// <param name="bitIndex"></param>
        /// <returns></returns>

        public static byte ResetBit(this byte b, int bitIndex)
        {
            if (-1 < bitIndex && bitIndex < 8)
                b = (byte)(b & ~(1 << bitIndex));
            else
                _IndexWarning("ResetBit", bitIndex);

            return b;
        }

        /// <summary>
        /// Fetches the binary representation of a byte.
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>

        public static string AsBinaryString(this byte b) => Convert.ToString(b, 2).PadLeft(8, '0');

        private static void _IndexWarning(string methodName, int bitIndex)
        {
            Serilog.Log.Warning($"[byte#{methodName}()] was provided an invalid bit index of {bitIndex}. Range is 0-7.");
        }
    }

}
