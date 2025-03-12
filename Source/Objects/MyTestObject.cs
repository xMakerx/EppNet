using EppNet.Attributes;
using EppNet.Data;

using System.Collections.Generic;
using System.Drawing;

namespace EppNet.Objects
{

    [NetworkObject(Dist = Distribution.Shared)]
    public partial class MyBaseObject : INetworkObject
    {

        [NetworkMethod]
        public void Goodbye() { }

        [NetworkMethod]
        public void YetAnotherMethod() { }

        [NetworkMethod]
        public void Hello(float b) { }
    }

    [NetworkObject(Dist = Distribution.Server)]
    public partial class MyTestObject : MyBaseObject
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
