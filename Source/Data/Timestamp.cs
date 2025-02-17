///////////////////////////////////////////////////////
/// Filename: Timestamp.cs
/// Date: February 16, 2025
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Node;
using EppNet.Utilities;

using System;
using System.Diagnostics.CodeAnalysis;

namespace EppNet.Data
{

    /// <summary>
    /// Essentially a tuple grouping the network time shown and the local monotonic time<br/>
    /// Comparisons are done using the monotonic time to keep timestamps unaffected by clock syncs
    /// </summary>
    public readonly struct Timestamp : IComparable, IComparable<Timestamp>, IEquatable<Timestamp>
    {

        public static readonly Timestamp Zero = new(TimeSpan.Zero, TimeSpan.Zero);

        /// <summary>
        /// The time displayed on the network clock
        /// </summary>
        public TimeSpan NetworkTime { get; }

        /// <summary>
        /// The time as specified by <see cref="ENet.Library.Time"/> which is the time since
        /// the internal ENet library started (in milliseconds)
        /// </summary>
        public TimeSpan MonotonicTime { get; }

        public Timestamp(TimeSpan netTime)
        {
            this.NetworkTime = netTime;
            this.MonotonicTime = TimeSpan.FromMilliseconds(ENet.Library.Time);
        }

        public Timestamp([NotNull] NetworkNode node)
        {
            Guard.AgainstNull(node);
            this.NetworkTime = node.Time;
            this.MonotonicTime = TimeSpan.FromMilliseconds(ENet.Library.Time);
        }

        public Timestamp([NotNull] INodeDescendant nodeDescendant)
        {
            Guard.AgainstNull(nodeDescendant);
            Guard.AgainstNull(nodeDescendant.Node);
            this.NetworkTime = nodeDescendant.Node.Time;
            this.MonotonicTime = TimeSpan.FromMilliseconds(ENet.Library.Time);
        }

        public Timestamp(TimeSpan netTime, TimeSpan monoTime)
        {
            this.NetworkTime = netTime;
            this.MonotonicTime = monoTime;
        }

        public int CompareTo(object obj)
            => obj is Timestamp ts ?
            CompareTo(ts) :
            0;

        public int CompareTo(Timestamp other)
            => other.MonotonicTime.CompareTo(MonotonicTime);

        public override bool Equals(object obj)
            => obj is Timestamp ts &&
            Equals(ts);

        public bool Equals(Timestamp other)
            => other.NetworkTime.Equals(NetworkTime) &&
            other.MonotonicTime.Equals(MonotonicTime);

        public override int GetHashCode()
            => NetworkTime.GetHashCode() ^ MonotonicTime.GetHashCode();

        public override string ToString()
            => $"Network(ticks): {NetworkTime.Ticks}\nMonotonic(ticks): {MonotonicTime.Ticks}";

        public static bool operator ==(Timestamp left, Timestamp right)
            => left.Equals(right);

        public static bool operator !=(Timestamp left, Timestamp right)
            => !left.Equals(right);

        public static bool operator >(Timestamp left, Timestamp right)
            => left.MonotonicTime > right.MonotonicTime;

        public static bool operator <(Timestamp left, Timestamp right)
            => left.MonotonicTime < right.MonotonicTime;

        public static bool operator >=(Timestamp left, Timestamp right)
            => left.MonotonicTime >= right.MonotonicTime;

        public static bool operator <=(Timestamp left, Timestamp right)
            => left.MonotonicTime <= right.MonotonicTime;
    }

}
