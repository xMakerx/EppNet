///////////////////////////////////////////////////////
/// Filename: IDatagram.cs
/// Date: September 12, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

namespace EppNet.Source.Data
{

    public interface IDatagram
    {

        public uint Header { internal set; get; }

        public void Read();

        public void WriteHeader();

    }

}
