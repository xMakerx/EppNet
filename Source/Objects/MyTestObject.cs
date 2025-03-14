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
        public void TupleMethod((bool, Distribution) b) { }

        [NetworkMethod]
        public void YetAnotherMethod(int b) { }

        [NetworkMethod]
        public void Hello(float[] b) { }

        [NetworkMethod]
        public void Test((bool, Distribution) t, List<MyTestObject> a, MyTestObject[] arr, Dictionary<Str8, int> b) { }
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

        [NetworkMethod]
        public void Greenbeans() { }
    }

    public static class LOL
    {
        [NetworkMethod]
        public static void Method() { }
    }

}
