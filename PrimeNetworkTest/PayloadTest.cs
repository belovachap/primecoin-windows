using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            var boop = new IntegerPayload(expected);
            Assert.AreEqual(payload, new IntegerPayload(expected));

            payload = new IntegerPayload(0xFC);
            expected = new byte[] {
                0xFC
            };
            AssertBytesEqual(expected, payload.ToBytes());
            Assert.AreEqual(payload, new IntegerPayload(expected));

            // When integer is <= 0xFFFF write 0xFD and value as uint16
            payload = new IntegerPayload(0x00FD);
            expected = new byte[] {
                0xFD,
                0xFD, 0x00
            };
            AssertBytesEqual(expected, payload.ToBytes());
            Assert.AreEqual(payload, new IntegerPayload(expected));

            payload = new IntegerPayload(0xFFFF);
            expected = new byte[] {
                0xFD,
                0xFF, 0xFF
            };
            AssertBytesEqual(expected, payload.ToBytes());
            Assert.AreEqual(payload, new IntegerPayload(expected));

            // When integer is <= 0xFFFFFFFF write 0xFE and value as uint32
            payload = new IntegerPayload(0x00010000);
            expected = new byte[] {
                0xFE,
                0x00, 0x00, 0x01, 0x00
            };
            AssertBytesEqual(expected, payload.ToBytes());
            Assert.AreEqual(payload, new IntegerPayload(expected));

            payload = new IntegerPayload(0xFFFFFFFF);
            expected = new byte[] {
                0xFE,
                0xFF, 0xFF, 0xFF, 0xFF
            };
            AssertBytesEqual(expected, payload.ToBytes());
            Assert.AreEqual(payload, new IntegerPayload(expected));

            // 0XFF and value as uint64
            payload = new IntegerPayload(0x0000000100000000);
            expected = new byte[] {
                0xFF,
                0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
            };
            AssertBytesEqual(expected, payload.ToBytes());
            Assert.AreEqual(payload, new IntegerPayload(expected));

            payload = new IntegerPayload(0xFFFFFFFFFFFFFFFF);
            expected = new byte[] {
                0xFF,
                0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
            };
            AssertBytesEqual(expected, payload.ToBytes());
            Assert.AreEqual(payload, new IntegerPayload(expected));
        }

        [TestMethod]
        public void TestStringPayloadToBytes()
        {
            StringPayload payload;
            byte[] expected;

            // Empty string should serialize as 0x00
            payload = new StringPayload("");
            expected = new byte[] {
                0x00,
            };
            AssertBytesEqual(expected, payload.ToBytes());

            payload = new StringPayload("testing.");
            expected = new byte[] {
                0x08,
                0x74, 0x65, 0x73, 0x74, 0x69, 0x6E, 0x67, 0x2E,
            };
            AssertBytesEqual(expected, payload.ToBytes());
        }

        [TestMethod]
        public void TestIPAddressPayloadToBytes()
        {
            IPAddressPayload payload;
            byte[] expected;

            payload = new IPAddressPayload(
               time_stamp: new DateTime((Int64)1000),
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
        }

        //[TestMethod]
        //public void TestVersoinPayloadToBytes()
        //{
        //    VersionPayload payload;
        //    byte[] expected;

        //    payload = new VersionPayload(
        //       time_stamp: new DateTime((Int64)1000),
        //       services: (UInt64)1,
        //       address_to: new IPAddressPayload(
        //           time_stamp: DateTime.UtcNow,
        //           services: (UInt64)1,
        //           address: IPAddress.Parse("10.0.0.2"),
        //           port: (UInt16)9911
        //       ),
        //       address_from: new IPAddressPayload(
        //           time_stamp: DateTime.UtcNow,
        //           services: (UInt64)1,
        //           address: IPAddress.Parse("10.0.0.1"),
        //           port: (UInt16)9911
        //       )
        //    );
        //    expected = new byte[] {
        //        0xE8, 0x03, 0x00, 0x00, // TimeStamp
        //        0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Services
        //        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0x0A, 0x00, 0x00, 0x01, // IPAddress
        //        0x20, 0x8D // Port
        //    };
        //    AssertBytesEqual(expected, payload.ToBytes());
        //}
    }
}
