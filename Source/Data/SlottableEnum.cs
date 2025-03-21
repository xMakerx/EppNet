﻿///////////////////////////////////////////////////////
/// Filename: SlottableEnum.cs
/// Date: July 29, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Utilities;

using System;
using System.Collections;

namespace EppNet.Data
{

    public struct SlottableEnum : IEquatable<SlottableEnum>, INameable, IConvertible, IComparable, IFormattable
    {

        #region Static access and operators

        internal static SlottableEnum _Internal_CreateAndAddTo(IList group, string name, uint slot)
        {
            Guard.AgainstNull(group);
            int index = group.Count;

            SlottableEnum nEnum = new(group, name, slot, 1 << index);
            group.Add(nEnum);
            return nEnum;
        }

        internal static void _Internal_ValidateEnums(SlottableEnum left, SlottableEnum right)
        {
            if (left.Group != right.Group)
                throw new InvalidOperationException("Enums must be from the same group!");

            if (left.Slot == right.Slot)
                throw new ArgumentException("Enums cannot be in the same slot!");
        }

        public static bool operator ==(SlottableEnum left, SlottableEnum right) => left.Equals(right);
        public static bool operator !=(SlottableEnum left, SlottableEnum right) => !left.Equals(right);

        public static SlottableEnum operator &(SlottableEnum left, SlottableEnum right)
        {
            _Internal_ValidateEnums(left, right);
            return new SlottableEnum(left.Group, string.Empty, 0, left.Value & right.Value);
        }

        public static SlottableEnum operator |(SlottableEnum left, SlottableEnum right)
        {
            _Internal_ValidateEnums(left, right);
            return new SlottableEnum(left.Group, string.Empty, 0, left.Value | right.Value);
        }

        public static SlottableEnum operator ^(SlottableEnum left, SlottableEnum right)
        {
            _Internal_ValidateEnums(left, right);
            return new SlottableEnum(left.Group, string.Empty, 0, left.Value ^ right.Value);
        }

        public static SlottableEnum operator ~(SlottableEnum e) => new SlottableEnum(e.Group, e.Name, 0, ~e.Value);

        public static bool operator <(SlottableEnum left, SlottableEnum right) => left.CompareTo(right) < 0;


        public static bool operator <=(SlottableEnum left, SlottableEnum right) => left.CompareTo(right) <= 0;

        public static bool operator >(SlottableEnum left, SlottableEnum right) => left.CompareTo(right) > 0;

        public static bool operator >=(SlottableEnum left, SlottableEnum right) => left.CompareTo(right) >= 0;

        #endregion

        public IList Group { internal set; get; }
        public string Name { set; get; }
        public uint Slot { internal set; get; }
        public int Value { internal set; get; }

        internal SlottableEnum(IList group, string name, uint slot, int value)
        {
            this.Group = group;
            this.Name = name;
            this.Slot = slot;
            this.Value = value;
        }

        public readonly bool Fits(SlottableEnum other) => other.Group == Group && other.Slot != Slot;

        public readonly bool IsOn(SlottableEnum other) => Fits(other) && (Value & other.Value) == other.Value;

        public readonly bool IsOff(SlottableEnum other) => !IsOn(other);

        public readonly TypeCode GetTypeCode() => TypeCode.Object;

        #region IConvertible

        public readonly bool ToBoolean(IFormatProvider provider) => Convert.ToBoolean(Value, provider);

        public readonly byte ToByte(IFormatProvider provider) => Convert.ToByte(Value, provider);

        public readonly char ToChar(IFormatProvider provider) => Convert.ToChar(Value, provider);

        public readonly DateTime ToDateTime(IFormatProvider provider) => Convert.ToDateTime(Value, provider);

        public readonly decimal ToDecimal(IFormatProvider provider) => Convert.ToDecimal(Value, provider);

        public readonly double ToDouble(IFormatProvider provider) => Convert.ToDouble(Value, provider);

        public readonly short ToInt16(IFormatProvider provider) => Convert.ToInt16(Value, provider);

        public readonly int ToInt32(IFormatProvider provider) => Convert.ToInt32(Value, provider);

        public readonly long ToInt64(IFormatProvider provider) => Convert.ToInt64(Value, provider);

        public readonly sbyte ToSByte(IFormatProvider provider) => Convert.ToSByte(Value, provider);

        public readonly float ToSingle(IFormatProvider provider) => Convert.ToSingle(Value, provider);

        public readonly string ToString(IFormatProvider provider) => Convert.ToString(Value, provider);

        public readonly object ToType(Type conversionType, IFormatProvider provider) => Convert.ChangeType(Value, conversionType, provider);

        public readonly ushort ToUInt16(IFormatProvider provider) => Convert.ToUInt16(Value, provider);

        public readonly uint ToUInt32(IFormatProvider provider) => Convert.ToUInt32(Value, provider);

        public readonly ulong ToUInt64(IFormatProvider provider) => Convert.ToUInt64(Value, provider);

        #endregion

        public readonly int CompareTo(object obj)
        {
            if (Group == null)
                throw new InvalidOperationException("Enum Group not defined!");

            if (obj is SlottableEnum other && Group.Contains(other))
                return Slot.CompareTo(other.Slot);

            return 1;
        }

        public readonly string ToString(string format, IFormatProvider formatProvider) => string.Format(formatProvider, format, Name);

        public readonly bool Equals(SlottableEnum other) => Name.Equals(other.Name, StringComparison.Ordinal)
            && Group == other.Group && Slot == other.Slot && Value == other.Value;

        public override readonly bool Equals(object obj)
        {
            if (obj is SlottableEnum other)
                return Equals(other);

            return false;
        }
        public override readonly int GetHashCode()
        {
            int hashCode = (Group?.GetHashCode()) ?? 0;
            hashCode ^= Name.GetHashCode();
            hashCode ^= Slot.GetHashCode();
            hashCode ^= Value.GetHashCode();
            return hashCode;
        }

    }

}
