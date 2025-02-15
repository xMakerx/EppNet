using EppNet.Attributes;
using EppNet.Node;
using EppNet.Sim;

using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;
using System.Text;

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
