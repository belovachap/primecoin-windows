﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrimeNetwork;
using System;
using System.Net;

namespace PrimeNetworkTest
{
    [TestClass]
    public class PayloadTest
    {
        void AssertBytesEqual(byte[] expected, byte[] actual)
        {
            Assert.AreEqual(expected.GetLength(0), actual.GetLength(0));
            for(int i = 0; i < expected.GetLength(0); i++)
            {
                Assert.AreEqual(expected[i], actual[i]);
            }
        }

        void AssertIntegerPayloadsEqual(IntegerPayload expected, IntegerPayload actual)
        {
            Assert.AreEqual(expected.Integer, actual.Integer);
        }

        void AssertStringPayloadsEqual(StringPayload expected, StringPayload actual)
        {
            Assert.AreEqual(expected.String, actual.String);
        }

        void AssertIPAddressPayloadsEqual(IPAddressPayload expected, IPAddressPayload actual)
        {
            Assert.AreEqual(expected.Address, actual.Address);
            Assert.AreEqual(expected.Port, actual.Port);
            Assert.AreEqual(expected.Services, actual.Services);
            Assert.AreEqual(expected.TimeStamp, actual.TimeStamp);
        }

        void AssertVersionPayloadsEqual(VersionPayload expected, VersionPayload actual)
        {
            Assert.AreEqual(expected.Version, actual.Version);
            Assert.AreEqual(expected.Services, actual.Services);
            Assert.AreEqual(expected.TimeStamp, actual.TimeStamp);
            AssertIPAddressPayloadsEqual(expected.AddressFrom, actual.AddressFrom);
            AssertIPAddressPayloadsEqual(expected.AddressTo, actual.AddressTo);
            Assert.AreEqual(expected.Nonce, actual.Nonce);
            AssertStringPayloadsEqual(expected.UserAgent, actual.UserAgent);
            Assert.AreEqual(expected.StartHeight, actual.StartHeight);
            Assert.AreEqual(expected.Relay, actual.Relay);
        }

        void AssertMessagePayloadsEqual(MessagePayload expected, MessagePayload actual)
        {
            Assert.AreEqual(expected.Magic, actual.Magic);
            Assert.AreEqual(expected.Command, actual.Command);
            AssertBytesEqual(
                expected.CommandPayload.ToBytes(),
                actual.CommandPayload.ToBytes()
            );
        }

        [TestMethod]
        public void TestIntegerPayloadFromBytes()
        {
            // Checks edge cases around instantiating from bytes.

            // No bytes won't work.
            Assert.ThrowsException<ArgumentException>(
                () => new IntegerPayload(new byte[] { })
            );

            // A single byte with the "flag" values won't work.
            Assert.ThrowsException<ArgumentOutOfRangeException>(
                () => new IntegerPayload(new byte[] { 0xFD })
            );
            Assert.ThrowsException<ArgumentOutOfRangeException>(
                () => new IntegerPayload(new byte[] { 0xFE })
            );
            Assert.ThrowsException<ArgumentOutOfRangeException>(
                () => new IntegerPayload(new byte[] { 0xFF })
            );
        }

        [TestMethod]
        public void TestIntegerPayload()
        {
            IntegerPayload payload;
            byte[] expected;
            
            // When integer is < 0xFD store it as uint8
            payload = new IntegerPayload(0x00);
            expected = new byte[] {
                0x00
            };
            AssertBytesEqual(expected, payload.ToBytes());
            AssertIntegerPayloadsEqual(payload, new IntegerPayload(expected));

            payload = new IntegerPayload(0xFC);
            expected = new byte[] {
                0xFC
            };
            AssertBytesEqual(expected, payload.ToBytes());
            AssertIntegerPayloadsEqual(payload, new IntegerPayload(expected));

            // When integer is <= 0xFFFF write 0xFD and value as uint16
            payload = new IntegerPayload(0x00FD);
            expected = new byte[] {
                0xFD,
                0xFD, 0x00
            };
            AssertBytesEqual(expected, payload.ToBytes());
            AssertIntegerPayloadsEqual(payload, new IntegerPayload(expected));

            payload = new IntegerPayload(0xFFFF);
            expected = new byte[] {
                0xFD,
                0xFF, 0xFF
            };
            AssertBytesEqual(expected, payload.ToBytes());
            AssertIntegerPayloadsEqual(payload, new IntegerPayload(expected));

            // When integer is <= 0xFFFFFFFF write 0xFE and value as uint32
            payload = new IntegerPayload(0x00010000);
            expected = new byte[] {
                0xFE,
                0x00, 0x00, 0x01, 0x00
            };
            AssertBytesEqual(expected, payload.ToBytes());
            AssertIntegerPayloadsEqual(payload, new IntegerPayload(expected));

            payload = new IntegerPayload(0xFFFFFFFF);
            expected = new byte[] {
                0xFE,
                0xFF, 0xFF, 0xFF, 0xFF
            };
            AssertBytesEqual(expected, payload.ToBytes());
            AssertIntegerPayloadsEqual(payload, new IntegerPayload(expected));

            // 0XFF and value as uint64
            payload = new IntegerPayload(0x0000000100000000);
            expected = new byte[] {
                0xFF,
                0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
            };
            AssertBytesEqual(expected, payload.ToBytes());
            AssertIntegerPayloadsEqual(payload, new IntegerPayload(expected));

            payload = new IntegerPayload(0xFFFFFFFFFFFFFFFF);
            expected = new byte[] {
                0xFF,
                0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
            };
            AssertBytesEqual(expected, payload.ToBytes());
            AssertIntegerPayloadsEqual(payload, new IntegerPayload(expected));
        }

        [TestMethod]
        public void TestStringPayload()
        {
            StringPayload payload;
            byte[] expected;

            // Empty string should serialize as 0x00
            payload = new StringPayload("");
            expected = new byte[] {
                0x00,
            };
            AssertBytesEqual(expected, payload.ToBytes());
            AssertStringPayloadsEqual(payload, new StringPayload(expected));

            payload = new StringPayload("testing.");
            expected = new byte[] {
                0x08,
                0x74, 0x65, 0x73, 0x74, 0x69, 0x6E, 0x67, 0x2E,
            };
            AssertBytesEqual(expected, payload.ToBytes());
            AssertStringPayloadsEqual(payload, new StringPayload(expected));
        }

        [TestMethod]
        public void TestIPAddressPayload()
        {
            IPAddressPayload payload;
            byte[] expected;

            payload = new IPAddressPayload(
               timeStamp: new DateTime(1000),
               services: (UInt64)1,
               address: IPAddress.Parse("10.0.0.1"),
               port: (UInt16)8333
            );
            expected = new byte[] {
                0xE8, 0x03, 0x00, 0x00, // TimeStamp
                0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Services
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0x0A, 0x00, 0x00, 0x01, // IPAddress
                0x20, 0x8D // Port
            };
            AssertBytesEqual(expected, payload.ToBytes());
            AssertIPAddressPayloadsEqual(payload, new IPAddressPayload(expected));
        }

        [TestMethod]
        public void TestVersoinPayload()
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
            AssertBytesEqual(expected, payload.ToBytes());
            AssertVersionPayloadsEqual(payload, new VersionPayload(expected));
        }

        [TestMethod]
        public void TestVerAckMessagePayload()
        {
            MessagePayload payload;
            byte[] expected;

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
            AssertBytesEqual(expected, payload.ToBytes());
            AssertMessagePayloadsEqual(payload, new MessagePayload(expected));
        }

        [TestMethod]
        public void TestVersionMessagePayload()
        {
            MessagePayload payload;
            byte[] expected;

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
            AssertBytesEqual(expected, payload.ToBytes());
            AssertMessagePayloadsEqual(payload, new MessagePayload(expected));

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
            AssertMessagePayloadsEqual(payload, new MessagePayload(expected));

            // A Length too short will result in an ArgumentException.
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
