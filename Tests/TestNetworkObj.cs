///////////////////////////////////////////////////////
/// Filename: TestNetworkObj.cs
/// Date: September 14, 2022
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Attributes;
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

        [Network()]
        public void Say(string message, int number)
        {
            string r = message + number;
           // Console.WriteLine($"Message: {message}, Favorite Number: {number}. I have {Apples} apples!");
        }


    }

}
