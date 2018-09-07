using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;

using Protocol;

namespace ProtocolTest
{
    [TestClass]
    public class TestVersionPayload
    {
        [TestMethod]
        public void TestSuccess()
        {
            VersionPayload payload;
            byte[] expected;

            payload = new VersionPayload(
                version: 70001,
                services: 0x01,
                timeStamp: new DateTime(1000),
                addressTo: new IPAddressPayload(
                    timeStamp: new DateTime(),
                    services: (UInt64)1,
                    address: IPAddress.Parse("10.0.0.2"),
                    port: (UInt16)9911
                ),
                addressFrom: new IPAddressPayload(
                    timeStamp: new DateTime(),
                    services: (UInt64)1,
                    address: IPAddress.Parse("10.0.0.1"),
                    port: (UInt16)9911
                ),
                nonce: 0,
                userAgent: new StringPayload("/UnitTest-v0.0.1/"),
                startHeight: 1000,
                relay: true
            );
            expected = new byte[] {
                0x71, 0x11, 0x01, 0x00,                         // Version
                0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Services 
                0xE8, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // TimeStamp
                
                // AddressTo
                0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Services
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Address...
                0x00, 0x00, 0xFF, 0xFF, 0x0A, 0x00, 0x00, 0x02, // ...
                0x26, 0xB7,                                     // Port
                
                // AddressFrom
                0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Services
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Address...
                0x00, 0x00, 0xFF, 0xFF, 0x0A, 0x00, 0x00, 0x01, // ...
                0x26, 0xB7,                                     // Port

                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Nonce

                // UserAgent
                17,                                             // UserAgent byte count
                0x2F, 0x55, 0x6E, 0x69, 0x74, 0x54, 0x65, 0x73, // "/UnitTest-v0.0.1/"...
                0x74, 0x2D, 0x76, 0x30, 0x2E, 0x30, 0x2E, 0x31, // ...
                0x2F,                                           // ...

                0xE8, 0x03, 0x00, 0x00,                         // StartHeight
                0x01                                            // Relay
            };
            TestPayload.AssertBytesEqual(expected, payload.ToBytes());
            TestPayload.AssertVersionPayloadsEqual(payload, new VersionPayload(expected));
        }
    }
}
