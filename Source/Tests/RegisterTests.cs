///////////////////////////////////////////////////////
/// Filename: RegisterTests.cs
/// Date: September 13, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////
using EppNet.Data.Datagrams;
using EppNet.Registers;

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
            bool fetched = _reg.TryGetNew(out PingDatagram _);
            Assert.IsTrue(fetched, "Did not instantiate new ping datagram properly!");
        }

        [TestMethod]
        public void TestAttributes()
        {



        }

    }

}
