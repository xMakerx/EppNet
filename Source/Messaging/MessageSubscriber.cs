/////////////////////////////////////////////
/// Filename: MessageSubscriber.cs
/// Date: November 26, 2024
/// Author: Maverick Liberty
//////////////////////////////////////////////
using System;

namespace EppNet.Messaging
{

    internal class MessageSubscriber<T> where T : class, IMessageHandler
    {
        public readonly T Subscriber;
        public readonly Subscription[] Subscriptions;
        public readonly bool SubscribedToAll;

        /// <summary>
        /// Denotes a subscriber to specified or all datagram types<br/>
        /// Passing null to subs indicates the passed subscriber is subscribed to all message types
        /// </summary>
        /// <param name="subscriber"></param>
        /// <param name="subs"></param>
        public MessageSubscriber(T subscriber, Subscription[] subs)
        {
            this.Subscriber = subscriber;
            this.Subscriptions = subs;
            this.SubscribedToAll = Subscriptions == null;
        }

    }

    internal struct Subscription
    {
        readonly Type DatagramType;
        readonly int Priority;
        public Subscription(Type dgType, int priority)
        {
            this.DatagramType = dgType;
            this.Priority = priority;
        }
    }

}
