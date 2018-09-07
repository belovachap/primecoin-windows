using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Numerics;

using Protocol;

namespace ProtocolTest
{
    [TestClass]
    public class TestAlgorithm
    {
        [TestMethod]
        public void TestPrimorialSuccess()
        {
            // 1# = 1
            Assert.AreEqual<BigInteger>(new BigInteger(1), Algorithm.Primorial(1));
            // 10# = 2 * 3 * 5 * 7 = 210
            Assert.AreEqual<BigInteger>(new BigInteger(210), Algorithm.Primorial(10));
        }

        [TestMethod]
        public void TestBlockRewardSuccess()
        {
            // Data from C++ Client on Testnet
            // TargetGetMint(nBits = 100546586) = 2781000000
            var difficulty = new DifficultyPayload(100546586);
            Assert.AreEqual<UInt64>(2781000000, Algorithm.BlockReward(difficulty));
        }

        [TestMethod]
        public void TestNextDifficultySuccess()
        {
            // Data from C++ Client on Testnet
            // TargetGetNext(nBits = 100546586, nInterval = 10080, nTargetSpacing = 60, nActualSpacing = 103)
            // nBitsNext = 100546570
            var expected = new DifficultyPayload(100546570);
            var actual = Algorithm.NextDifficulty(new DifficultyPayload(100546586), 103);
            TestPayload.AssertDifficulyPayloadsEqual(expected, actual);

            // Data from C++ Client on Testnet
            // TargetGetNext(nBits = 113526130, nInterval = 10080, nTargetSpacing = 60, nActualSpacing = 6)
            // nBitsNext = 113526829
            expected = new DifficultyPayload(113526829);
            actual = Algorithm.NextDifficulty(new DifficultyPayload(113526130), 6);
            TestPayload.AssertDifficulyPayloadsEqual(expected, actual);

            // Data from C++ Client on Testnet
            // TargetGetNext(nBits = 113524601, nInterval = 10080, nTargetSpacing = 60, nActualSpacing = -1)
            // nBitsNext = 113525391
            expected = new DifficultyPayload(113525391);
            actual = Algorithm.NextDifficulty(new DifficultyPayload(113524601), -1);
            TestPayload.AssertDifficulyPayloadsEqual(expected, actual);

            // Data from C++ Client on Testnet
            // TargetGetNext(nBits = 113527199, nInterval = 10080, nTargetSpacing = 60, nActualSpacing = 373)
            // nBitsNext = 113523149
            expected = new DifficultyPayload(113523149);
            actual = Algorithm.NextDifficulty(new DifficultyPayload(113527199), 373);
            TestPayload.AssertDifficulyPayloadsEqual(expected, actual);
        }

        [TestMethod]
        public void TestCheckProofOfWorkFailure()
        {
            // Target Chain Length too small
            var block = new BlockPayload(
                version: 2,
                previousBlockHash: new Byte[] { },
                timeStamp: 1,
                bits: 0x05000000,
                nonce: 1,
                primeChainMultiplier: new BigInteger(),
                txs: null
            );
            var protocolConfig = ProtocolConfiguration.MainnetConfig();
            Assert.ThrowsException<Algorithm.TargetChainLengthTooSmall>(
                () => Algorithm.CheckProofOfWork(block, protocolConfig)
            );

            // Target Chain Length too big
            block = new BlockPayload(
               version: 2,
               previousBlockHash: new Byte[] { },
               timeStamp: 1,
               bits: 0x64000000,
               nonce: 1,
               primeChainMultiplier: new BigInteger(),
               txs: null
            );
            Assert.ThrowsException<Algorithm.TargetChainLengthTooBig>(
                () => Algorithm.CheckProofOfWork(block, protocolConfig)
            );

            // Header Hash too small
            var txInputs = new List<TxInputPayload>();
            var txOutputs = new List<TxOutputPayload>();
            var txs = new List<TransactionPayload>();

            txInputs.Add(
                new TxInputPayload(
                    previousTx: new TxOutPointPayload(
                        txHash: new Byte[] {
                            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        },
                        index: 0xFFFFFFFF
                    ),
                    script: new UnknownPayload(
                        bytes: new Byte[] {
                            0x03, 0x15, 0x86, 0x29, 0x01, 0x4F, 0x00, 0x06,
                            0x2F, 0x50, 0x32, 0x53, 0x48, 0x2F,
                        }
                    ),
                    sequence: 0xFFFFFFFF
                )
            );

            txOutputs.Add(
                new TxOutputPayload(
                    amount: 740000000,
                    script: new UnknownPayload(
                        bytes: new Byte[] {
                            0x21, 0x02, 0xF8, 0x2B, 0xD5, 0x4C, 0xF1, 0xFA,
                            0x3D, 0x07, 0xA1, 0xC3, 0x74, 0xD1, 0x79, 0x02,
                            0x46, 0xED, 0x82, 0x13, 0x23, 0x52, 0x29, 0xEC,
                            0x86, 0xA3, 0x95, 0x02, 0xE3, 0x19, 0x0B, 0x07,
                            0x1A, 0xBB, 0xAC,
                        }
                    )
                )
            );

            txs.Add(
                new TransactionPayload(
                    version: 1,
                    txInputs: txInputs,
                    txOutputs: txOutputs,
                    lockTime: 0
                )
            );

            block = new BlockPayload(
                version: 2,
                previousBlockHash: new Byte[] {
                    0xAF, 0x20, 0xAC, 0xD1, 0xC5, 0x63, 0x22, 0x2A,
                    0x94, 0x2F, 0x18, 0xD5, 0x97, 0x59, 0xE5, 0xD1,
                    0xEC, 0x5B, 0xD1, 0x16, 0xE5, 0x9C, 0x68, 0x11,
                    0x88, 0x3A, 0x85, 0xB6, 0x6D, 0x2C, 0x4A, 0x6F,
                },
                timeStamp: 0x5B315EF6,
                bits: 0x0B9D8796,
                nonce: 0x00000000,
                primeChainMultiplier: BigInteger.Parse("11450505506960965632"),
                txs: txs
            );
            Assert.ThrowsException<Algorithm.BlockHeaderHashTooSmall>(
                () => Algorithm.CheckProofOfWork(block, protocolConfig)
            );

            block = new BlockPayload(
                version: 2,
                previousBlockHash: new Byte[] {
                    0xAF, 0x20, 0xAC, 0xD1, 0xC5, 0x63, 0x22, 0x2A,
                    0x94, 0x2F, 0x18, 0xD5, 0x97, 0x59, 0xE5, 0xD1,
                    0xEC, 0x5B, 0xD1, 0x16, 0xE5, 0x9C, 0x68, 0x11,
                    0x88, 0x3A, 0x85, 0xB6, 0x6D, 0x2C, 0x4A, 0x6F,
                },
                timeStamp: 0x5B315EF6,
                bits: 0x0B9D8796,
                nonce: 0x5ED830A2,
                primeChainMultiplier: BigInteger.Parse("0"),
                txs: txs
            );
            Assert.ThrowsException<Algorithm.ChainOriginTooSmall>(
                () => Algorithm.CheckProofOfWork(block, protocolConfig)
            );

            block = new BlockPayload(
                version: 2,
                previousBlockHash: new Byte[] {
                    0xAF, 0x20, 0xAC, 0xD1, 0xC5, 0x63, 0x22, 0x2A,
                    0x94, 0x2F, 0x18, 0xD5, 0x97, 0x59, 0xE5, 0xD1,
                    0xEC, 0x5B, 0xD1, 0x16, 0xE5, 0x9C, 0x68, 0x11,
                    0x88, 0x3A, 0x85, 0xB6, 0x6D, 0x2C, 0x4A, 0x6F,
                },
                timeStamp: 0x5B315EF6,
                bits: 0x0B9D8796,
                nonce: 0x5ED830A2,
                primeChainMultiplier: new BigInteger(1) << 2000 - 1,
                txs: txs
            );
            Assert.ThrowsException<Algorithm.ChainOriginTooBig>(
                () => Algorithm.CheckProofOfWork(block, protocolConfig)
            );
        }
    }
}
