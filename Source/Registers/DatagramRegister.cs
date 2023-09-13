//////////////////////////////////////////////
/// Filename: DatagramRegister.cs
/// Date: September 13, 2022
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Data;

namespace EppNet.Registers
{

    public class DatagramRegister : Register<byte, IDatagram>
    {

        public static readonly DatagramRegister Instance = new DatagramRegister();
        public static DatagramRegister Get() => Instance;

        public DatagramRegister() : base()
        {
            Add<PingDatagram>(0x1);
        }
    }

}
