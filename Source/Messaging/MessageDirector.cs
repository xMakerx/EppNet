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
            Type[] interfaces = subscriber.GetType().GetInterfaces();
            Type[] messagesTypes = null;

            for (int i = 0; i < interfaces.Length; i++)
            {
                Type iT = interfaces[i];

                if (iT.IsAssignableFrom(typeof(IMessageHandler)) && iT.GetGenericArguments().Length > 0)
                {
                    messagesTypes = iT.GetGenericArguments();
                    break;
                }
            }

            // TODO: This
            if (messagesTypes == null)
                return false;
            // We want all datagrams. Kinda weird

            return true;
        }
    }


}