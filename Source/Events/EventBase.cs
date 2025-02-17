///////////////////////////////////////////////////////
/// Filename: EventBase.cs
/// Date: February 14, 2025
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Data;
using EppNet.Node;
using EppNet.Time;
using EppNet.Utilities;

using System;
using System.Diagnostics.CodeAnalysis;

namespace EppNet.Events
{

    public interface IEventBase : ITimestamped, IComparable, IComparable<IEventBase>, IEquatable<IEventBase>, INodeDescendant
    {
        public object Sender { get; }

        public object GetSubject();

    }

    public abstract class EventBase<TSubject> : IEventBase, IComparable<EventBase<TSubject>>, IEquatable<EventBase<TSubject>>
    {

        public NetworkNode Node { get; }
        public TSubject Subject { get; }
        public Timestamp Timestamp { get; }
        public object Sender { get; }

        protected EventBase([NotNull] NetworkNode node, [NotNull] TSubject subject, object sender)
        {
            Guard.AgainstNull(node, subject);
            this.Node = node;
            this.Timestamp = node.Timestamp;
            this.Subject = subject;
            this.Sender = sender;
        }

        protected EventBase([NotNull] NetworkNode node, [NotNull] TSubject subject) : this(node, subject, null) { }

        public object GetSubject() => Subject;

        public override bool Equals(object obj)
            => (obj is EventBase<TSubject> other &&
            Equals(other)) ||
            (obj is IEventBase otherEB) &&
            Equals(otherEB);

        public bool Equals(IEventBase other)
            => other is not null &&
            other.Node == Node &&
            other.Timestamp == Timestamp &&
            ReferenceEquals(other.GetSubject(), Subject) &&
            other.Sender == Sender;

        public bool Equals(EventBase<TSubject> other)
            => other is not null &&
            other.Node == Node &&
            other.Timestamp == Timestamp &&
            ReferenceEquals(other.Subject, Subject) &&
            other.Sender == Sender;

        public override int GetHashCode()
            => Node.GetHashCode() ^
            Timestamp.GetHashCode() ^
            Subject.GetHashCode() ^
            ((Sender != null) ? Sender.GetHashCode() : 1);

        public int CompareTo(object obj)
            => obj is EventBase<TSubject> other ?
            CompareTo(other) :
            obj is IEventBase otherEB ?
            CompareTo(otherEB) :
            0;

        public int CompareTo(IEventBase other)
            => other is not null ?
            Timestamp.CompareTo(other.Timestamp) :
            0;

        public int CompareTo(EventBase<TSubject> other)
            => other is not null ? 
            Timestamp.CompareTo(other.Timestamp) :
            0;

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