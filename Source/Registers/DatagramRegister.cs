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

        public DatagramRegister() : base()
        {
            Add<PingDatagram>(0x0);
        }
    }

}
