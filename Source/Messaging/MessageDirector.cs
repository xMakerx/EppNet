/////////////////////////////////////////////
/// Filename: MessageDirector.cs
/// Date: July 11, 2024
/// Author: Maverick Liberty
//////////////////////////////////////////////

using EppNet.Services;

using System;

namespace EppNet.Messaging
{

    public class MessageDirector : Service
    {

        public MessageDirector(ServiceManager svcMgr) : base(svcMgr)
        {

        }

        public bool Subscribe<T>(T subscriber) where T : IMessageHandler
        {
            Type[] messagesTypes;

            // We need to extract the datagram types from the message handler
            foreach (Type intType in subscriber.GetType().GetInterfaces())
            {

                // Ensure interface derives from IMessageHandler
                if (!typeof(IMessageHandler).IsAssignableFrom(intType))
                    continue;

                if (intType.IsGenericType)
                    // Handler has specific datagrams it's interested in
                    messagesTypes = intType.GetGenericArguments();
                else
                    // Subscribe to all datagram types
                    messagesTypes = Array.Empty<Type>();
            }

            // TODO: Subscribe

            return true;
        }
    }


}