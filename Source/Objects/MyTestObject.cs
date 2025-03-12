using EppNet.Attributes;
using EppNet.Data;

using System.Collections.Generic;
using System.Drawing;

namespace EppNet.Objects
{

    [NetworkObject(Dist = Distribution.Client)]
    public partial class MyBaseObject : INetworkObject
    {

        [NetworkMethod]
        public void Goodbye() { }

    }

    [NetworkObject(Dist = Distribution.Server)]
    public partial class MyTestObject : MyBaseObject
    {
        public long ID { set; get; }

        [NetworkMethod]
        public void Hello((MyBaseObject, decimal) a)
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
