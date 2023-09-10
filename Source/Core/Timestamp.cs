///////////////////////////////////////////////////////
/// Filename: Timestamp.cs
/// Date: September 10, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////
using System;

namespace EppNet.Core
{

    public enum TimestampType : byte
    {
        None            = 0,
        Milliseconds    = 1,
        Seconds         = 2,
        Minutes         = 3
    }

    /// <summary>
    /// Wrapper class to handle longs that represent timestamps
    /// Why not a <see cref="TimeSpan"/>? Good question. The idea behind here is to
    /// have a simple wrapper around the primitive numeric types 
    /// </summary>
    
    public struct Timestamp : IComparable, IEquatable<Timestamp>
    {

        #region Static members
        private static Timestamp _CreateMatching(Timestamp a, Timestamp b, bool alwaysNew = false)
        {
            if (a.Type == b.Type && !alwaysNew)
                return b;

            Timestamp nts = new Timestamp(b.Type, b.Monotonic, b.Value);
            nts.SetType(a.Type);

            return nts;
        }

        public static Timestamp UTCMilliseconds()
        {
            Timestamp nts = new Timestamp()
            {
                Type = TimestampType.Milliseconds,
                Value = ((long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds),
                Monotonic = false
            };

            return nts;
        }

        /// <summary>
        /// Creates a new monotonic timestamp representing 0 seconds.
        /// </summary>
        /// <returns></returns>
        public static Timestamp ZeroMonotonicMs() => new Timestamp(TimestampType.Milliseconds, true, 0);

        #endregion

        private TimestampType _type;

        public TimestampType Type
        {
            set
            {
                SetType(value);
            }

            get => _type;
        }

        /// <summary>
        /// Whether or not the value of this timestamp is monotonic
        /// </summary>
        public bool Monotonic;

        public long Value;

        public Timestamp()
        {
            this._type = TimestampType.None;
            this.Monotonic = false;
            this.Value = 0;
        }

        public Timestamp(TimestampType type, bool monotonic, long value)
        {
            this._type = type;
            this.Monotonic = monotonic;
            this.Value = value;
        }

        public Timestamp(int value) : this()
        {
            this.Value = value;
        }

        public Timestamp(uint value) : this()
        {
            this.Value = value;
        }

        public Timestamp(float value) : this()
        {
            this.Value = (long)value;
        }

        public Timestamp(ulong value) : this()
        {
            this.Value = (long)value;
        }

        public Timestamp(long value) : this()
        {
            this.Value = value;
        }

        public void Set(int value) => this.Value = value;
        public void Set(uint value) => this.Value = value;
        public void Set(long value) => this.Value = value;
        public void Set(ulong value) => this.Value = (long)value;
        public void Set(float value) => this.Value = (long)value;

        public void SetToMonoNow()
        {
            if (this._type == TimestampType.None || this._type == TimestampType.Milliseconds)
            {
                Set(Network.MonotonicTime);
                return;
            }

            Timestamp b = _CreateMatching(this, Network.MonotonicTimestamp);
            Set(b.Value);
        }

        public long Get() => this.Value;

        /// <summary>
        /// Sets the type of time this timestamp represents and converts the
        /// value held by it.
        /// NOTE: This performs integer division!!! Be very careful!
        /// </summary>
        /// <param name="type"></param>

        public void SetType(TimestampType type)
        {

            if (Type == TimestampType.None || Type == type)
            {
                // We don't have to do any conversion!
                this._type = type;
                return;
            }

            switch (Type)
            {
                case TimestampType.Milliseconds:

                    if (type == TimestampType.Seconds)
                        this.Value /= 1000;

                    else if (type == TimestampType.Minutes)
                        this.Value = (Value / 1000) / 60;

                    break;

                case TimestampType.Seconds:

                    if (type == TimestampType.Milliseconds)
                        this.Value *= 1000;

                    else if (type == TimestampType.Minutes)
                        this.Value /= 60;

                    break;

                case TimestampType.Minutes:

                    if (type == TimestampType.Milliseconds)
                        this.Value *= 60000;

                    else if (type == TimestampType.Seconds)
                        this.Value *= 60;

                    break;

                default: break;

            }

            this._type = type;
        }

        private Timestamp _CreateMatching(Timestamp other) => Timestamp._CreateMatching(this, other);

        public int CompareTo(object obj)
        {
            if (!(obj is Timestamp))
                return -1;

            Timestamp b = _CreateMatching((Timestamp)obj);

            if (this.Value < b.Value)
                return -1;

            if (this.Value > b.Value)
                return 1;

            return 0;

        }

        /// <summary>
        /// Checks if the specified <see cref="Timestamp"/> represents the same amount of time regardless
        /// of if it's monotonic or not. This differs from the == and != operators as this does conversions
        /// prior to comparing. (i.e. Timestamp a could be representing 2 minutes and timestamp b could be representing 120 seconds, you would
        /// get a true)
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>

        public override bool Equals(object obj)
        {

            if (obj is Timestamp)
            {
                Timestamp b = _CreateMatching((Timestamp)obj);
                return (Value == b.Value);
            }

            return false;
        }

        /// <summary>
        /// Checks if the specified <see cref="Timestamp"/> represents the same amount of time regardless
        /// of if it's monotonic or not. This differs from the == and != operators as this does conversions
        /// prior to comparing. (i.e. Timestamp a could be representing 2 minutes and timestamp b could be representing 120 seconds, you would
        /// get a true)
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>

        public bool Equals(Timestamp other)
        {
            Timestamp b = _CreateMatching(other);
            return (Value == b.Value);
        }

        /// <summary>
        /// Combines the time unit, whether or not this is monotonic, and the value.
        /// Use this for the ultimate equality check
        /// </summary>
        /// <returns></returns>

        public override int GetHashCode() => HashCode.Combine(Type, Monotonic, Value);

        #region Operators
        public static implicit operator Timestamp(int time) => new Timestamp(time);
        public static implicit operator Timestamp(uint time) => new Timestamp(time);
        public static implicit operator Timestamp(float time) => new Timestamp(time);
        public static implicit operator Timestamp(ulong time) => new Timestamp(time);
        public static implicit operator Timestamp(long time) => new Timestamp(time);

        public static implicit operator int(Timestamp a) => (int)a.Value;
        public static implicit operator float(Timestamp a) => (float)a.Value;
        public static implicit operator ulong(Timestamp a) => (ulong)a.Value;
        public static implicit operator long(Timestamp a) => (long)a.Value;

        #region Comparison operators
        public static bool operator <(Timestamp a, Timestamp b) => a.CompareTo(b) < 0;
        public static bool operator >(Timestamp a, Timestamp b) => a.CompareTo(b) > 0;
        public static bool operator <=(Timestamp a, Timestamp b) => a.CompareTo(b) <= 0;
        public static bool operator >=(Timestamp a, Timestamp b) => a.CompareTo(b) >= 0;

        /// <summary>
        /// Java-like equality comparator. Both timestamps must be equivalent in unit of time and in value.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator ==(Timestamp a, Timestamp b) => a.Type == b.Type && a.Value == b.Value;

        /// <summary>
        /// Java-like inequality comparator. Both timestamps must nbt be equivalent in unit of time and in value.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator !=(Timestamp a, Timestamp b) => !(a.Type == b.Type && a.Value == b.Value);
        #endregion

        #region Addition operators

        public static uint operator +(Timestamp a, uint time) => (uint)a.Value + time;
        public static int operator +(Timestamp a, int time) => (int)a.Value + time;
        public static float operator +(Timestamp a, float time) => (float)a.Value + time;
        public static ulong operator +(Timestamp a, ulong time) => (ulong)a.Value + time;
        public static long operator +(Timestamp a, long time) => (long)a.Value + time;
        public static Timestamp operator +(Timestamp a, Timestamp b)
        {
            b = Timestamp._CreateMatching(a, b, true);
            b.Value += a.Value;
            return b;
        }

        #endregion

        #region Subtraction operators
        public static uint operator -(Timestamp a, uint b) => (uint) a.Value - b;
        public static int operator -(Timestamp a, int b) => (int)a.Value - b;
        public static float operator -(Timestamp a, float b) => (float)a.Value - b;
        public static ulong operator -(Timestamp a, ulong b) => (ulong)a.Value - b;
        public static long operator -(Timestamp a, long b) => (long)a.Value - b;
        public static Timestamp operator -(Timestamp a, Timestamp b)
        {
            Timestamp c = Timestamp._CreateMatching(a, b, true);
            c.Value = a.Value - c.Value;
            return c;
        }

        #endregion

        #region Multiplication operators
        public static uint operator *(Timestamp a, uint b) => (uint)a.Value * b;
        public static int operator *(Timestamp a, int b) => (int)a.Value * b;
        public static float operator *(Timestamp a, float b) => (float)a.Value * b;
        public static ulong operator *(Timestamp a, ulong b) => (ulong)a.Value * b;
        public static long operator *(Timestamp a, long b) => (long)a.Value * b;

        public static Timestamp operator *(Timestamp a, Timestamp b)
        {
            b = Timestamp._CreateMatching(a, b, true);
            b.Value *= a.Value;

            return b;
        }

        #endregion

        #region Division operators

        public static uint operator /(Timestamp a, uint b) => (uint)a.Value / b;
        public static int operator /(Timestamp a, int b) => (int)a.Value / b;
        public static float operator /(Timestamp a, float b) => (float)a.Value / b;
        public static ulong operator /(Timestamp a, ulong b) => (ulong)a.Value / b;
        public static long operator /(Timestamp a, long b) => (long)a.Value / b;

        public static Timestamp operator /(Timestamp a, Timestamp b)
        {
            Timestamp c = Timestamp._CreateMatching(a, b, true);
            c.Value = (a.Value / c.Value);
            return c;
        }

        #endregion

        #endregion

    }

}
