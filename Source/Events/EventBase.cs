///////////////////////////////////////////////////////
/// Filename: EventBase.cs
/// Date: February 14, 2025
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Node;
using EppNet.Utilities;

using System;
using System.Diagnostics.CodeAnalysis;

namespace EppNet.Events
{

    public abstract class EventBase<TSubject> : INodeDescendant, IEquatable<EventBase<TSubject>>
    {

        public NetworkNode Node { get; }
        public TSubject Subject { get; }
        public TimeSpan Timestamp { get; }
        public object Sender { get; }

        protected EventBase([NotNull] NetworkNode node, [NotNull] TSubject subject, object sender)
        {
            Guard.AgainstNull(node, subject);
            this.Node = node;
            this.Timestamp = node.Time;
            this.Subject = subject;
            this.Sender = sender;
        }

        protected EventBase([NotNull] NetworkNode node, [NotNull] TSubject subject) : this(node, subject, null) { }

        public bool Equals(EventBase<TSubject> other)
            => other != null &&
            other.Node == Node &&
            other.Timestamp == Timestamp &&
            other.Subject.Equals(Subject) &&
            other.Sender == Sender;

        public override bool Equals(object obj)
            => obj is EventBase<TSubject> other && Equals(other);

        public override int GetHashCode()
            => Node.GetHashCode() ^
            Timestamp.GetHashCode() ^
            Subject.GetHashCode() ^
            ((Sender != null) ? Sender.GetHashCode() : 1);

        public static bool operator ==(EventBase<TSubject> a, EventBase<TSubject> b)
        {
            if (a == null && b == null)
                return true;

            else if (a == null || b == null)
                return false;

            return a.Equals(b);
        }

        public static bool operator !=(EventBase<TSubject> a, EventBase<TSubject> b)
            => !(a == b);
    }

}