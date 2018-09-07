using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;

using Protocol;

namespace ProtocolTest
{
    [TestClass]
    public class TestIPAddressPayload
    {
        [TestMethod]
        public void TestSuccess()
        {
            IPAddressPayload payload;
            Byte[] expected;

            payload = new IPAddressPayload(
               timeStamp: new DateTime(1000),
               services: (UInt64)1,
               address: IPAddress.Parse("10.0.0.1"),
               port: (UInt16)8333
            );
            expected = new Byte[] {
                0xE8, 0x03, 0x00, 0x00, // TimeStamp
                0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Services
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0x0A, 0x00, 0x00, 0x01, // IPAddress
                0x20, 0x8D // Port
            };
            TestPayload.AssertBytesEqual(expected, payload.ToBytes());
            TestPayload.AssertIPAddressPayloadsEqual(payload, new IPAddressPayload(expected));
        }
    }
}
