using Microsoft.VisualStudio.TestTools.UnitTesting;
using Protocol;

namespace ProtocolTest
{
    static class TestPayload
    {
        public static void AssertBytesEqual(byte[] expected, byte[] actual)
        {
            Assert.AreEqual(expected.GetLength(0), actual.GetLength(0));
            for (int i = 0; i < expected.GetLength(0); i++)
            {
                Assert.AreEqual(expected[i], actual[i]);
            }
        }

        public static void AssertDifficulyPayloadsEqual(DifficultyPayload expected, DifficultyPayload actual)
        {
            Assert.AreEqual(expected.Length, actual.Length);
            Assert.AreEqual(expected.Fraction, actual.Fraction);
        }

        public static void AssertIntegerPayloadsEqual(IntegerPayload expected, IntegerPayload actual)
        {
            Assert.AreEqual(expected.Integer, actual.Integer);
        }

        public static void AssertStringPayloadsEqual(StringPayload expected, StringPayload actual)
        {
            Assert.AreEqual(expected.String, actual.String);
        }

        public static void AssertIPAddressPayloadsEqual(IPAddressPayload expected, IPAddressPayload actual)
        {
            Assert.AreEqual(expected.Address, actual.Address);
            Assert.AreEqual(expected.Port, actual.Port);
            Assert.AreEqual(expected.Services, actual.Services);
            Assert.AreEqual(expected.TimeStamp, actual.TimeStamp);
        }

        public static void AssertVersionPayloadsEqual(VersionPayload expected, VersionPayload actual)
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

        public static void AssertMessagePayloadsEqual(MessagePayload expected, MessagePayload actual)
        {
            Assert.AreEqual(expected.Magic, actual.Magic);
            Assert.AreEqual(expected.Command, actual.Command);
            AssertBytesEqual(
                expected.CommandPayload.ToBytes(),
                actual.CommandPayload.ToBytes()
            );
        }

        public static void AssertBlockPayloadsEqual(BlockPayload expected, BlockPayload actual)
        {
            Assert.AreEqual(expected.Version, actual.Version);
            AssertBytesEqual(expected.PreviousBlockHash, actual.PreviousBlockHash);
            AssertBytesEqual(expected.MerkleRoot, actual.MerkleRoot);
            Assert.AreEqual(expected.TimeStamp, actual.TimeStamp);
            Assert.AreEqual(expected.Bits, actual.Bits);
            Assert.AreEqual(expected.Nonce, actual.Nonce);
            Assert.AreEqual(expected.PrimeChainMultiplier, actual.PrimeChainMultiplier);
        }
    }
}
