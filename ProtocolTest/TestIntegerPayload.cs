using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Protocol;

namespace ProtocolTest
{
    [TestClass]
    public class TestIntegerPayload
    {
        [TestMethod]
        public void TestFailure()
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
        public void TestSuccess()
        {
            IntegerPayload payload;
            byte[] expected;

            // When integer is < 0xFD store it as uint8
            payload = new IntegerPayload(0x00);
            expected = new byte[] {
                0x00
            };
            TestPayload.AssertBytesEqual(expected, payload.ToBytes());
            TestPayload.AssertIntegerPayloadsEqual(payload, new IntegerPayload(expected));

            payload = new IntegerPayload(0xFC);
            expected = new byte[] {
                0xFC
            };
            TestPayload.AssertBytesEqual(expected, payload.ToBytes());
            TestPayload.AssertIntegerPayloadsEqual(payload, new IntegerPayload(expected));

            // When integer is <= 0xFFFF write 0xFD and value as uint16
            payload = new IntegerPayload(0x00FD);
            expected = new byte[] {
                0xFD,
                0xFD, 0x00
            };
            TestPayload.AssertBytesEqual(expected, payload.ToBytes());
            TestPayload.AssertIntegerPayloadsEqual(payload, new IntegerPayload(expected));

            payload = new IntegerPayload(0xFFFF);
            expected = new byte[] {
                0xFD,
                0xFF, 0xFF
            };
            TestPayload.AssertBytesEqual(expected, payload.ToBytes());
            TestPayload.AssertIntegerPayloadsEqual(payload, new IntegerPayload(expected));

            // When integer is <= 0xFFFFFFFF write 0xFE and value as uint32
            payload = new IntegerPayload(0x00010000);
            expected = new byte[] {
                0xFE,
                0x00, 0x00, 0x01, 0x00
            };
            TestPayload.AssertBytesEqual(expected, payload.ToBytes());
            TestPayload.AssertIntegerPayloadsEqual(payload, new IntegerPayload(expected));

            payload = new IntegerPayload(0xFFFFFFFF);
            expected = new byte[] {
                0xFE,
                0xFF, 0xFF, 0xFF, 0xFF
            };
            TestPayload.AssertBytesEqual(expected, payload.ToBytes());
            TestPayload.AssertIntegerPayloadsEqual(payload, new IntegerPayload(expected));

            // 0XFF and value as uint64
            payload = new IntegerPayload(0x0000000100000000);
            expected = new byte[] {
                0xFF,
                0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
            };
            TestPayload.AssertBytesEqual(expected, payload.ToBytes());
            TestPayload.AssertIntegerPayloadsEqual(payload, new IntegerPayload(expected));

            payload = new IntegerPayload(0xFFFFFFFFFFFFFFFF);
            expected = new byte[] {
                0xFF,
                0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
            };
            TestPayload.AssertBytesEqual(expected, payload.ToBytes());
            TestPayload.AssertIntegerPayloadsEqual(payload, new IntegerPayload(expected));
        }
    }
}
