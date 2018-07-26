using Microsoft.VisualStudio.TestTools.UnitTesting;
using Protocol;

namespace ProtocolTest
{
    [TestClass]
    public class TestStringPayload
    {
        [TestMethod]
        public void TestSuccess()
        {
            StringPayload payload;
            byte[] expected;

            // Empty string should serialize as 0x00
            payload = new StringPayload("");
            expected = new byte[] {
                0x00,
            };
            TestPayload.AssertBytesEqual(expected, payload.ToBytes());
            TestPayload.AssertStringPayloadsEqual(payload, new StringPayload(expected));

            payload = new StringPayload("testing.");
            expected = new byte[] {
                0x08,
                0x74, 0x65, 0x73, 0x74, 0x69, 0x6E, 0x67, 0x2E,
            };
            TestPayload.AssertBytesEqual(expected, payload.ToBytes());
            TestPayload.AssertStringPayloadsEqual(payload, new StringPayload(expected));
        }
    }
}
