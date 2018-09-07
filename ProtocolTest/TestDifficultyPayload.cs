using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

using Protocol;

namespace ProtocolTest
{
    [TestClass]
    public class TestDifficultyPayload
    {
        [TestMethod]
        public void TestDifficultyPayloadSuccess()
        {
            // Data from C++ Client on Testnet
            // TargetGetLength(nBits = 113526130) = 6
            // TargetGetFractional(nBits = 113526130) = 12862834
            // TargetGetFractionalDifficulty(nBits = 113526130) = 18408421568
            var difficulty = new DifficultyPayload(113526130);
            Assert.AreEqual(6, difficulty.Length);
            Assert.AreEqual<UInt32>(12862834, difficulty.Fraction);
            Assert.AreEqual<UInt64>(18408421568, difficulty.GetFractionalDifficulty());

            // Data from C++ Client on Testnet
            // TargetGetLength(nBits = 100546586) = 5
            // TargetGetFractional(nBits = 100546586) = 16660506
            // TargetGetFractionalDifficulty(nbits = 100546586) = 617407197651
            difficulty = new DifficultyPayload(100546586);
            Assert.AreEqual(5, difficulty.Length);
            Assert.AreEqual<UInt32>(16660506, difficulty.Fraction);
            Assert.AreEqual<UInt64>(617407197651, difficulty.GetFractionalDifficulty());
        }
    }
}
