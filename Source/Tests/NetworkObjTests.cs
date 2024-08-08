///////////////////////////////////////////////////////
/// Filename: NetworkObjTests.cs
/// Date: September 13, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////
using EppNet.Objects;

namespace EppNet.Tests
{

    [TestClass]
    public class NetworkObjTests
    {

        private ObjectRegistration _test_reg;
        private TestNetworkObj obj;

        private int Runs = 10000;

        public NetworkObjTests()
        {

            //_test_reg = new ObjectRegistration<TestNetworkObj>();
            _test_reg.Compile();

            obj = (TestNetworkObj)_test_reg.NewInstance(7);
        }

    }
}