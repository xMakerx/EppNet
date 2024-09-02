///////////////////////////////////////////////////////
/// Filename: SequenceNumber.cs
/// Date: August 18, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;

namespace EppNet.Snapshots
{

    /// <summary>
    /// Sequence numbers are wraparound numbers <br/>
    /// Current range is 0 -> 127
    /// </summary>

    public readonly struct SequenceNumber : IComparable, IFormattable, IEquatable<SequenceNumber>
    {

        public const sbyte MinValue = 0;
        public const sbyte QuartValue = sbyte.MaxValue / 4;
        public const sbyte MidValue = sbyte.MaxValue / 2;
        public const sbyte MaxValue = sbyte.MaxValue;

        public readonly sbyte Value;

        /// <summary>
        /// An out of range integer is clamped
        /// </summary>
        /// <param name="value"></param>

        public SequenceNumber(int value)
        {
            // Clamp the values
            if (value > MaxValue)
                value = MaxValue;

            else if (value < MinValue)
                value = MinValue;

            this.Value = (sbyte) value;
        }

        public SequenceNumber(sbyte value)
        {
            this.Value = value;
        }

        public readonly int CompareTo(object obj)
        {
            if (obj is SequenceNumber other)
                return CompareTo(other);

            return 1;
        }

        public readonly int CompareTo(SequenceNumber other)
        {

            if (Value >= MidValue && other.Value < QuartValue)
                return -1;

            if (other.Value >= MidValue && Value < QuartValue)
                return 1;

            return Value.CompareTo(other.Value);
        }

        public readonly SequenceNumber Next()
        {
            if (Value + 1 > MaxValue)
                return new();

            return new(Value + 1);
        }

        public readonly SequenceNumber Previous()
        {
            if (Value - 1 < MinValue)
                return new SequenceNumber(MaxValue);

            return new(Value - 1);
        }

        public readonly string ToString(string format, IFormatProvider formatProvider) => Value.ToString(format, formatProvider);

        public override readonly bool Equals(object obj)
        {
            if (obj is SequenceNumber other)
                return Equals(other);

            return false;
        }

        public readonly bool Equals(SequenceNumber other) => Value.Equals(other.Value);

        public override readonly int GetHashCode() => Value.GetHashCode();

        #region Operators
        public static SequenceNumber operator ++(SequenceNumber a) => a.Next();
        public static SequenceNumber operator --(SequenceNumber a) => a.Previous();
        public static bool operator ==(SequenceNumber left, SequenceNumber right) => left.Equals(right);
        public static bool operator !=(SequenceNumber left, SequenceNumber right) => !left.Equals(right);
        public static bool operator <(SequenceNumber left, SequenceNumber right) => left.CompareTo(right) < 0;
        public static bool operator <=(SequenceNumber left, SequenceNumber right) => left.CompareTo(right) <= 0;
        public static bool operator >(SequenceNumber left, SequenceNumber right) => left.CompareTo(right) > 0;
        public static bool operator >=(SequenceNumber left, SequenceNumber right) => left.CompareTo(right) >= 0;

        #endregion
    }

}