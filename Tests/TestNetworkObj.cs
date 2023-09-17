///////////////////////////////////////////////////////
/// Filename: TestNetworkObj.cs
/// Date: September 14, 2022
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Attributes;
using EppNet.Objects;
using EppNet.Sim;

using System;

namespace EppNet.Tests
{

    public class TestNetworkObj : ISimUnit
    {

        public int Apples;

        public TestNetworkObj(int apples)
        {
            this.Apples = apples;
        }

        public void AnnounceGenerate()
        {
            throw new NotImplementedException();
        }

        public void Generate()
        {
            throw new NotImplementedException();
        }

        public long GetID()
        {
            throw new NotImplementedException();
        }

        public ObjectDelegate GetObjectDelegate()
        {
            throw new NotImplementedException();
        }

        public bool RequestDelete()
        {
            throw new NotImplementedException();
        }

        [NetworkMethod()]
        public void Say(string message, int number)
        {
            string r = message + number;
           // Console.WriteLine($"Message: {message}, Favorite Number: {number}. I have {Apples} apples!");
        }

        public bool SendUpdate(string methodName, params object[] args)
        {
            throw new NotImplementedException();
        }

        void ISimUnit.AnnounceGenerate()
        {
            throw new NotImplementedException();
        }

        void ISimUnit.Generate()
        {
            throw new NotImplementedException();
        }

        long ISimUnit.GetID()
        {
            throw new NotImplementedException();
        }

        ObjectDelegate ISimUnit.GetObjectDelegate()
        {
            throw new NotImplementedException();
        }

        bool ISimUnit.RequestDelete()
        {
            throw new NotImplementedException();
        }

        void ISimUnit.SetID(long id)
        {
            throw new NotImplementedException();
        }

        void ISimUnit.SetObjectDelegate(ObjectDelegate oDelegate)
        {
            throw new NotImplementedException();
        }
    }

}
