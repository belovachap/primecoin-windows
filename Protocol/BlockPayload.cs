using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;

namespace Protocol
{
    public class BlockPayload : Payload
    {

        public Int32 Version { get; }
        public Byte[] PreviousBlockHash { get; }
        public UInt32 TimeStamp { get; }
        public UInt32 Bits { get; }
        public UInt32 Nonce;
        public BigInteger PrimeChainMultiplier;
        public List<TransactionPayload> Transactions { get; }
        public Byte[] MerkleRoot { get { return Algorithm.MerkleRoot(Transactions); } }

        public BlockPayload(
            Int32 version,
            Byte[] previousBlockHash,
            UInt32 timeStamp,
            UInt32 bits,
            UInt32 nonce,
            BigInteger primeChainMultiplier,
            List<TransactionPayload> txs
        )
        {
            Version = version;
            PreviousBlockHash = previousBlockHash;
            TimeStamp = timeStamp;
            Bits = bits;
            Nonce = nonce;
            PrimeChainMultiplier = primeChainMultiplier;
            Transactions = txs;
        }

        public BlockPayload(byte[] bytes)
        {
            Version = BitConverter.ToInt32(bytes, 0);
            var remaining = bytes.Skip(4);

            PreviousBlockHash = remaining.Take(32).ToArray();
            remaining = remaining.Skip(32);

            var merkleRoot = remaining.Take(32).ToArray();
            remaining = remaining.Skip(32);

            TimeStamp = BitConverter.ToUInt32(remaining.ToArray(), 0);
            remaining = remaining.Skip(4);

            Bits = BitConverter.ToUInt32(remaining.ToArray(), 0);
            remaining = remaining.Skip(4);

            Nonce = BitConverter.ToUInt32(remaining.ToArray(), 0);
            remaining = remaining.Skip(4);

            // Handle PrimeChainMultiplier...
            var pcmCount = new IntegerPayload(remaining.ToArray());
            remaining = remaining.Skip(pcmCount.ToBytes().Length);
            var pcmBytes = remaining.Take((Int32)pcmCount.Integer).ToArray();
            remaining = remaining.Skip(pcmBytes.Length);
            PrimeChainMultiplier = new BigInteger(pcmBytes);

            var txCount = new IntegerPayload(remaining.ToArray());
            remaining = remaining.Skip(txCount.ToBytes().Length);

            Transactions = new List<TransactionPayload>();
            for (UInt64 i = 0; i < txCount.Integer; i++)
            {
                var tx = new TransactionPayload(remaining.ToArray());
                remaining = remaining.Skip(tx.ToBytes().Length);
                Transactions.Add(tx);
            }

            if (!merkleRoot.SequenceEqual(MerkleRoot))
            {
                throw new Exception("MerkleRoot incorrect!");
            }
        }

        public override byte[] ToBytes()
        {
            var bytes = new Byte[0].AsEnumerable();
            bytes = bytes.Concat(BitConverter.GetBytes(Version));
            bytes = bytes.Concat(PreviousBlockHash);
            bytes = bytes.Concat(MerkleRoot);
            bytes = bytes.Concat(BitConverter.GetBytes(TimeStamp));
            bytes = bytes.Concat(BitConverter.GetBytes(Bits));
            bytes = bytes.Concat(BitConverter.GetBytes(Nonce));

            var pcmBytes = PrimeChainMultiplier.ToByteArray();
            var pcmCount = new IntegerPayload((UInt64)pcmBytes.Length);
            bytes = bytes.Concat(pcmCount.ToBytes()).Concat(pcmBytes);

            var txCount = new IntegerPayload((UInt64)Transactions.Count);
            bytes = bytes.Concat(txCount.ToBytes());
            foreach (TransactionPayload tx in Transactions)
            {
                bytes = bytes.Concat(tx.ToBytes());
            }

            return bytes.ToArray();
        }

        public Byte[] HeaderHash()
        {
            var version = BitConverter.GetBytes(Version);
            var timeStamp = BitConverter.GetBytes(TimeStamp);
            var bits = BitConverter.GetBytes(Bits);
            var nonce = BitConverter.GetBytes(Nonce);
            var bytes = version
                         .Concat(PreviousBlockHash)
                         .Concat(MerkleRoot)
                         .Concat(timeStamp)
                         .Concat(bits)
                         .Concat(nonce)
                         .ToArray();

            SHA256 sha256 = SHA256Managed.Create();
            return sha256.ComputeHash(sha256.ComputeHash(bytes));
        }

        public Byte[] Hash()
        {
            var version = BitConverter.GetBytes(Version);
            var timeStamp = BitConverter.GetBytes(TimeStamp);
            var bits = BitConverter.GetBytes(Bits);
            var nonce = BitConverter.GetBytes(Nonce);
            var pcmBytes = PrimeChainMultiplier.ToByteArray();
            var pcmCount = new IntegerPayload((UInt64)pcmBytes.Length);
            var pcm = pcmCount.ToBytes().Concat(pcmBytes);
            var bytes = version
                         .Concat(PreviousBlockHash)
                         .Concat(MerkleRoot)
                         .Concat(timeStamp)
                         .Concat(bits)
                         .Concat(nonce)
                         .Concat(pcm)
                         .ToArray();

            SHA256 sha256 = SHA256Managed.Create();
            return sha256.ComputeHash(sha256.ComputeHash(bytes));
        }
    }
}
