using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol
{
    public class TxOutPointPayload : Payload
    {
        public Byte[] TransactionHash { get; }
        public UInt32 Index { get; }

        public TxOutPointPayload(Byte[] txHash, UInt32 index)
        {
            TransactionHash = txHash;
            Index = index;
        }

        public TxOutPointPayload(byte[] bytes)
        {
            TransactionHash = bytes.Take(32).ToArray();
            Index = BitConverter.ToUInt32(bytes, 32);
        }

        public override byte[] ToBytes()
        {
            return TransactionHash.Concat(BitConverter.GetBytes(Index)).ToArray();
        }
    }
}
