///////////////////////////////////////////////////////
/// Filename: ISnapshot.cs
/// Date: July 22, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Time;

using System;

namespace EppNet.Snapshots
{

    /// <summary>
    /// Base class for snapshots; representations of something at a particular time
    /// </summary>

    public abstract class SnapshotBase : IComparable, IEquatable<SnapshotBase>
    {

        /// <summary>
        /// The length of the randomly generated header
        /// </summary>
        public const int HeaderLength = 8;

        #region Static Access and Operators

        private static Random _rand = new Random();

        /// <summary>
        /// Generates a random header for this state <br/>
        /// Thanks to Dan Rigby: https://stackoverflow.com/a/1344258/26439978
        /// </summary>
        /// <returns>Randomly generated header<br/>
        /// - Example: 6Q4j9CgP
        /// </returns>
        private static string _Internal_GenerateHeader()
        {
            string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            char[] chars = new char[HeaderLength];

            for (int i = 0; i < chars.Length; i++)
                chars[i] = validChars[_rand.Next(validChars.Length)];

            return new string(chars);
        }

        public static bool operator ==(SnapshotBase lhs, SnapshotBase rhs) => lhs?.Equals(rhs) == true;
        public static bool operator !=(SnapshotBase lhs, SnapshotBase rhs) => !(lhs?.Equals(rhs)) == false;
        public static bool operator <(SnapshotBase lhs, SnapshotBase rhs) => lhs.CompareTo(rhs) < 0;
        public static bool operator >(SnapshotBase lhs, SnapshotBase rhs) => lhs.CompareTo(rhs) > 0;
        public static bool operator <=(SnapshotBase lhs, SnapshotBase rhs) => lhs.CompareTo(rhs) <= 0;
        public static bool operator >=(SnapshotBase lhs, SnapshotBase rhs) => lhs.CompareTo(rhs) >= 0;

        #endregion


        /// <summary>
        /// The header identifies this snapshot. It's randomly generated [Aa-Zz, 0-9]
        /// </summary>

        public readonly string Header;
        public readonly Timestamp Timestamp;

        protected SnapshotBase()
        {
            this.Header = _Internal_GenerateHeader();
            this.Timestamp = Timestamp.FromMonoNow();
        }

        protected SnapshotBase(string header, Timestamp timestamp)
        {
            this.Header = string.IsNullOrEmpty(header) ? _Internal_GenerateHeader() : header;
            this.Timestamp = timestamp == 0L ? Timestamp.FromMonoNow() : timestamp;
        }

        /// <summary>
        /// Records the current state of what we're capturing
        /// </summary>
        public abstract void RecordCurrent();

        /// <summary>
        /// See <see cref="ulong.CompareTo(ulong)"/><br/>
        /// This function is similar but null or undefined comparisons return 1.
        /// </summary>
        /// <param name="other"></param>
        /// <returns>Time comparison</returns>

        public int CompareTo(SnapshotBase other)
        {

            if (other == null)
                return 1;

            return Timestamp.CompareTo(other.Timestamp);
        }

        /// <summary>
        /// See <see cref="ulong.CompareTo(ulong)"/><br/>
        /// This function is similar but null or undefined comparisons return 1 or error
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>See <see cref="ulong.CompareTo(ulong)"/></returns>
        /// <exception cref="ArgumentException">Undefined comparison, object isn't of SnapshotBase</exception>

        public virtual int CompareTo(object obj)
        {
            if (obj == null)
                return 1;

            if (obj is SnapshotBase snapshot)
                return CompareTo(snapshot);

            throw new ArgumentException($"Cannot compare a {obj.GetType().Name} with a {this.GetType().Name}!", nameof(obj));
        }

        /// <summary>
        /// Checks if the specified object is a <see cref="SnapshotBase"/> and we have equivalent headers and timestamps
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>Whether or not the header and timestamp are equal</returns>

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            return (obj is SnapshotBase snapshot) && Equals(snapshot);
        }

        /// <summary>
        /// Checks for equivalent headers and timestamps
        /// </summary>
        /// <param name="other"></param>
        /// <returns>Whether or not the header and timestamp are equal</returns>

        public bool Equals(SnapshotBase other)
        {
            if (other == null)
                return false;

            return other.Header == Header && other.Timestamp == Timestamp;
        }

        /// <summary>
        /// The hash code of our header
        /// </summary>
        /// <returns>Hash code of our header</returns>

        public override int GetHashCode() => Header.GetHashCode();

    }


}