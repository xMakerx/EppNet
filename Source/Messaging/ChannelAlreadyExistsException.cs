/////////////////////////////////////////////
/// Filename: ChannelAlreadyExistsException.cs
/// Date: July 11, 2024
/// Author: Maverick Liberty
//////////////////////////////////////////////

using System;

namespace EppNet.Messaging
{

    public class ChannelAlreadyExistsException : Exception
    {

        public readonly byte Id;

        public ChannelAlreadyExistsException(byte id, string message) : base(message)
        {
            this.Id = id;
        }

    }

}