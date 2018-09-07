using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;

using Protocol;

namespace ProtocolTest
{
    [TestClass]
    public class TestMessagePayload
    {
        [TestMethod]
        public void TestVerAckSuccess()
        {
            MessagePayload payload;
            Byte[] expected;

            var commandPayload = new VerAckPayload();

            UInt32 magic = 0xE7E5E7E4;

            payload = new MessagePayload(magic, "verack", commandPayload);
            expected = new byte[] {

                0xE4, 0xE7, 0xE5, 0xE7,                         // Primecoin Magic Bytes
                0x76, 0x65, 0x72, 0x61, 0x63, 0x6B, 0x00, 0x00, // verack Command zero padded...
                0x00, 0x00, 0x00, 0x00,                         // ... 
                0x00, 0x00, 0x00, 0x00,                         // Length (0)
                0x5D, 0xF6, 0xE0, 0xE2,                         // CheckSum
            };
            TestPayload.AssertBytesEqual(expected, payload.ToBytes());
            TestPayload.AssertMessagePayloadsEqual(payload, new MessagePayload(expected));
        }

        [TestMethod]
        public void TestVersionSuccess()
        {
            MessagePayload payload;
            Byte[] expected;

            var commandPayload = new VersionPayload(
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

            UInt32 magic = 0xE7E5E7E4;

            payload = new MessagePayload(magic, "version", commandPayload);
            expected = new byte[] {
                0xE4, 0xE7, 0xE5, 0xE7,                         // Primecoin Magic Bytes
                0x76, 0x65, 0x72, 0x73, 0x69, 0x6F, 0x6E, 0x00, // version Command zero padded ...
                0x00, 0x00, 0x00, 0x00,                         // ...
                0x67, 0x00, 0x00, 0x00,                         // Length
                0xF2, 0xC8, 0x73, 0x0E,                         // Checksum

                // VersionPayload
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
            TestPayload.AssertMessagePayloadsEqual(payload, new MessagePayload(expected));

            // The Relay byte is optional on receive, defaults to false.
            commandPayload = new VersionPayload(
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
                relay: false
            );
            payload = new MessagePayload(magic, "version", commandPayload);
            expected = new byte[] {
                0xE4, 0xE7, 0xE5, 0xE7,                         // Primecoin Magic Bytes
                0x76, 0x65, 0x72, 0x73, 0x69, 0x6F, 0x6E, 0x00, // version Command zero padded ...
                0x00, 0x00, 0x00, 0x00,                         // ...
                0x66, 0x00, 0x00, 0x00,                         // Length
                0x08, 0x24, 0x09, 0x21,                         // Checksum

                // VersionPayload
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
            };
            TestPayload.AssertMessagePayloadsEqual(payload, new MessagePayload(expected));
        }

        [TestMethod]
        public void TestVersionFailure()
        {
            UInt32 magic = 0xE7E5E7E4;

            // A Length too short will result in an ArgumentException.
            var commandPayload = new VersionPayload(
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
                relay: false
            );
            var payload = new MessagePayload(magic, "version", commandPayload);
            var expected = new Byte[] {
                0xE4, 0xE7, 0xE5, 0xE7,                         // Primecoin Magic Bytes
                0x76, 0x65, 0x72, 0x73, 0x69, 0x6F, 0x6E, 0x00, // version Command zero padded ...
                0x00, 0x00, 0x00, 0x00,                         // ...
                0x65, 0x00, 0x00, 0x00,                         // Length
                0x08, 0x24, 0x09, 0x21,                         // Checksum

                // VersionPayload
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
            };
            Assert.ThrowsException<ArgumentException>(() => new MessagePayload(expected));

            // A Length too long will result in an ArgumentException.
            commandPayload = new VersionPayload(
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
                relay: false
            );
            payload = new MessagePayload(magic, "version", commandPayload);
            expected = new byte[] {
                0xE4, 0xE7, 0xE5, 0xE7,                         // Primecoin Magic Bytes
                0x76, 0x65, 0x72, 0x73, 0x69, 0x6F, 0x6E, 0x00, // version Command zero padded ...
                0x00, 0x00, 0x00, 0x00,                         // ...
                0x67, 0x00, 0x00, 0x00,                         // Length
                0x08, 0x24, 0x09, 0x21,                         // Checksum

                // VersionPayload
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
            };
            Assert.ThrowsException<ArgumentException>(() => new MessagePayload(expected));
        }
    }
}
