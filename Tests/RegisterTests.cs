///////////////////////////////////////////////////////
/// Filename: RegisterTests.cs
/// Date: September 13, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Data;
using EppNet.Registers;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EppNet.Tests
{

    [TestClass]
    public class RegisterTests
    {

        private DatagramRegister _reg;

        public RegisterTests()
        {
            this._reg = DatagramRegister.Get();
            _reg.Compile();
        }

        [TestMethod]
        public void TestPingDatagramGen()
        {
            IDatagram d = (IDatagram) _reg.Get(0x1).NewInstance();

            Assert.IsTrue(d is PingDatagram, "Did not instantiate new ping datagram properly!");
        }

        [TestMethod]
        public void TestAttributes()
        {



        }

    }

}
