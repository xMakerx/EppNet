///////////////////////////////////////////////////////
/// Filename: IDatagram.cs
/// Date: September 12, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Connections;

namespace EppNet.Data.Datagrams
{

    public interface IDatagram
    {

        public bool Written { get; }

        public void Read();

        public void Write();

        public byte[] Pack();

        public void WriteHeader();
        public byte GetHeader();
        public byte GetChannelID();
        public Connection GetSender();

    }

}
