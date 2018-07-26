using Microsoft.VisualStudio.TestTools.UnitTesting;

using Protocol;

namespace ProtocolTest
{
    [TestClass]
    public class TestDifficultyPayload
    {
        [TestMethod]
        public void TestSuccess()
        {
            var difficulty = new DifficultyPayload(0x0B9D8796);
            Assert.AreEqual(0x0B, difficulty.Length);
        }
    }
}
