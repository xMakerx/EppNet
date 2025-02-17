using EppNet.Attributes;

namespace EppNet.Objects
{

    [NetworkObject(Dist = Distribution.Server)]
    public partial class MyTestObject : INetworkObject
    {
        public long ID { set; get; }

        public void Hello()
        {
        }
    }

}
