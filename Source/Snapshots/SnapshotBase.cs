///////////////////////////////////////////////////////
/// Filename: SnapshotBase.cs
/// Date: July 22, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Node;
using EppNet.Utilities;

using System;

namespace EppNet.Snapshots
{

    /// <summary>
    /// Base class for snapshots; representations of something at a particular time
    /// </summary>

    public abstract class SnapshotBase : INodeDescendant, IComparable, IEquatable<SnapshotBase>, IComparable<SnapshotBase>
    {

        #region Static Access and Operators

        public static bool operator ==(SnapshotBase lhs, SnapshotBase rhs) => lhs?.Equals(rhs) == true;
        public static bool operator !=(SnapshotBase lhs, SnapshotBase rhs) => lhs?.Equals(rhs) != true;
        public static bool operator <(SnapshotBase lhs, SnapshotBase rhs) => lhs?.CompareTo(rhs) < 0;
        public static bool operator >(SnapshotBase lhs, SnapshotBase rhs) => lhs?.CompareTo(rhs) > 0;
        public static bool operator <=(SnapshotBase lhs, SnapshotBase rhs) => lhs?.CompareTo(rhs) <= 0;
        public static bool operator >=(SnapshotBase lhs, SnapshotBase rhs) => lhs?.CompareTo(rhs) >= 0;

        #endregion

        public SequenceNumber LocalSequence { get; }

        public long GlobalSequence { protected set; get; }

        public TimeSpan Timestamp { get; }

        public NetworkNode Node { get; }

        /// <summary>
        /// The next snapshot
        /// </summary>
        public SnapshotBase Next { internal set; get; }

        /// <summary>
        /// The previous snapshot
        /// </summary>
        public SnapshotBase Previous { internal set; get; }

        protected SnapshotBase(NetworkNode node, TimeSpan timestamp, SequenceNumber localSequence)
        {
            Guard.AgainstNull(node, nameof(node));
            this.Node = node;
            this.LocalSequence = localSequence;
            this.Timestamp = timestamp;
        }

        protected SnapshotBase(NetworkNode node, long globalSequence, SequenceNumber localSequence)
        {
            Guard.AgainstNull(node, nameof(node));
            this.Node = node;
            this.GlobalSequence = globalSequence;
            this.LocalSequence = localSequence;
            this.Timestamp = node.Time;
            
            // Let's init our doubly "linked list"
            this.Next = null;
            this.Previous = null;
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
            => (other != null)
            ? GlobalSequence.CompareTo(other.GlobalSequence)
            : 1;

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
        /// Checks if the specified object is a <see cref="SnapshotBase"/> and we have equivalent headers
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>Whether or not the header and timestamp</returns>

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            return (obj is SnapshotBase snapshot) && Equals(snapshot);
        }

        /// <summary>
        /// Checks for equivalent headers
        /// </summary>
        /// <param name="other"></param>
        /// <returns>Whether or not the headers are equal</returns>

        public bool Equals(SnapshotBase other)
        {
            if (other == null)
                return false;

            return other.LocalSequence == LocalSequence;
        }

        /// <summary>
        /// The hash code of our header
        /// </summary>
        /// <returns>Hash code of our header</returns>

        public override int GetHashCode() => LocalSequence.GetHashCode();

    }


}