using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Numerics;

using Protocol;

namespace ProtocolTest
{
    [TestClass]
    public class TestBlockPayload
    {
        [TestMethod]
        public void TestSuccess()
        {
            BlockPayload payload;
            byte[] expected;

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

            payload = new BlockPayload(
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
                primeChainMultiplier: BigInteger.Parse("11450505506960965632"),
                txs: txs
            );

            expected = new byte[] {
                0x02, 0x00, 0x00, 0x00,				            // Version

                0xAF, 0x20, 0xAC, 0xD1, 0xC5, 0x63, 0x22, 0x2A, // PreviousBlockHash...
                0x94, 0x2F, 0x18, 0xD5, 0x97, 0x59, 0xE5, 0xD1, // ...
                0xEC, 0x5B, 0xD1, 0x16, 0xE5, 0x9C, 0x68, 0x11, // ...
                0x88, 0x3A, 0x85, 0xB6, 0x6D, 0x2C, 0x4A, 0x6F, // ...

                0x30, 0xE5, 0x3D, 0x4F, 0x8B, 0xB1, 0x81, 0xFB, // MerkleRoot...
                0x21, 0x2A, 0xB9, 0x6A, 0x4F, 0x68, 0xF2, 0x4E, // ...
                0xB6, 0x3A, 0x6F, 0xA0, 0x04, 0x8F, 0x2E, 0x1F, // ...
                0x35, 0x75, 0x55, 0x5C, 0x4D, 0xC9, 0x5B, 0x5A, // ...

                0xF6, 0x5E, 0x31, 0x5B,			                // TimeStamp
                0x96, 0x87, 0x9D, 0x0B,                         // Bits
                0xA2, 0x30, 0xD8, 0x5E,                         // Nonce
                
                // PrimeChainMultiplier (OpenSSL BIGNUM mpi format)
                0x09,                                           // PrimeChainMultiplier byte count
				0x00, 0x00, 0x10, 0x59, 0x17, 0x5E, 0xE8, 0x9E, // PrimeChainMultiplier Little Endian...
                0x00,                                           // ...

                0x01,                                           // Transactions count

                // Transaction
                0x01, 0x00, 0x00, 0x00,                         // Transaction Version

                0x01,                                           // Transaction Inputs count
                // Transaction Input (coinbase)
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // PreviousTransactionOutput...
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // ...
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // ...
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // ...
                0xFF, 0xFF, 0xFF, 0xFF,
                0x0E,                                           // Script length
                0x03, 0x15, 0x86, 0x29, 0x01, 0x4F, 0x00, 0x06, // Script...
                0x2F, 0x50, 0x32, 0x53, 0x48, 0x2F,             // ...
                0xFF, 0xFF, 0xFF, 0xFF,                         // Sequence

                0x01,                                           // Transaction Outputs count
                // Transaction Output
                0x00, 0x81, 0x1B, 0x2C, 0x00, 0x00, 0x00, 0x00, // Amount
                0x23,                                           // Script length
                0x21, 0x02, 0xF8, 0x2B, 0xD5, 0x4C, 0xF1, 0xFA, // Script...
                0x3D, 0x07, 0xA1, 0xC3, 0x74, 0xD1, 0x79, 0x02, // ...
                0x46, 0xED, 0x82, 0x13, 0x23, 0x52, 0x29, 0xEC, // ...
                0x86, 0xA3, 0x95, 0x02, 0xE3, 0x19, 0x0B, 0x07, // ...
                0x1A, 0xBB, 0xAC,                               // ...

                0x00, 0x00, 0x00, 0x00 		                    // LockTime
            };
            TestPayload.AssertBytesEqual(expected, payload.ToBytes());
            TestPayload.AssertBlockPayloadsEqual(payload, new BlockPayload(expected));

            var protocolConfig = new ProtocolConfiguration(
                magic: 0xE7E5E7E4,
                minimumChainLength: 6,
                maximumChainLength: 99,
                minimumHeaderHash: new BigInteger(1) << 255,
                minimumChainOrigin: new BigInteger(1) << 255,
                maximumChainOrigin: new BigInteger(1) << 2000 - 1
            );
            Algorithm.CheckProofOfWork(payload, protocolConfig); // Should be a valid block.
        }
    }
}
