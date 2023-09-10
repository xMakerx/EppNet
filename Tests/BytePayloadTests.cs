///////////////////////////////////////////////////////
/// Filename: BytePayloadTests.cs
/// Date: September 10, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Data;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EppNet.Tests
{

    [TestClass]
    public class BytePayloadTests
    {

        [TestMethod]
        public void ReadAndWriteString()
        {
            BytePayload payloadOut = new BytePayload();

            string input = "Hello World!";
            payloadOut.WriteString(input);

            byte[] bufferIn = payloadOut.Pack();
            payloadOut.Dispose();

            BytePayload payloadIn = new BytePayload(bufferIn);
            Assert.AreEqual(input, payloadIn.ReadString());
        }

    }

}