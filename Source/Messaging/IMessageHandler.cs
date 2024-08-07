/////////////////////////////////////////////
/// Filename: IMessageHandler.cs
/// Date: August 6, 2024
/// Author: Maverick Liberty
//////////////////////////////////////////////

using EppNet.Data.Datagrams;

namespace EppNet.Messaging
{

    public interface IMessageHandler
    {
        public void Handle(IDatagram message);
    }

    public interface IMessageHandler<in T> : IMessageHandler where T : IDatagram { }

    public interface IMesasgeHandler<in T1, in T2> : IMessageHandler where T1 : IDatagram where T2 : IDatagram { }

}