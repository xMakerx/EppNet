using EppNet.Attributes;
using EppNet.Data;

using System.Collections.Generic;

namespace EppNet.Objects
{

    [NetworkObject(Dist = Distribution.Server)]
    public partial class MyTestObject : INetworkObject
    {
        public long ID { set; get; }

        [NetworkMethod]
        public void Hello(int a)
        {

        }

        [NetworkMethod]
        public void Method(Distribution a) { }
    }

    public static class LOL
    {
        [NetworkMethod]
        public static void Method() { }
    }

}
