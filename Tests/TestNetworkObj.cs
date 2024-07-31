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
        public long ID { set; get; }

        public TestNetworkObj(int apples)
        {
            this.Apples = apples;
        }
        public void AnnounceGenerate()
        {
            throw new NotImplementedException();
        }

        public void OnGenerate()
        {
            throw new NotImplementedException();
        }

        public bool RequestDelete()
        {
            throw new NotImplementedException();
        }

        public void OnDelete() { }

        [NetworkMethod()]
        public void Say(string message, int number)
        {
            string r = message + number;
           // Console.WriteLine($"Message: {message}, Favorite Number: {number}. I have {Apples} apples!");
        }

    }

}
