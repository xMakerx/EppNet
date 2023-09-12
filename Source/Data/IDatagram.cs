///////////////////////////////////////////////////////
/// Filename: IDatagram.cs
/// Date: September 12, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Connections;

namespace EppNet.Data
{

    public interface IDatagram
    {

        public void Read();

        public void WriteHeader();
        public byte GetHeader();
        public byte GetChannelID();
        public Connection GetSender();

    }

}
