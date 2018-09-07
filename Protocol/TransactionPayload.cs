using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol
{
    public class TransactionPayload : Payload
    {

        public Int32 Version { get; }
        public List<TxInputPayload> TxInputs;
        public List<TxOutputPayload> TxOutputs;
        public UInt32 LockTime;

        public TransactionPayload(
            Int32 version,
            List<TxInputPayload> txInputs,
            List<TxOutputPayload> txOutputs,
            UInt32 lockTime
        )
        {
            Version = version;
            TxInputs = txInputs;
            TxOutputs = txOutputs;
            LockTime = lockTime;
        }

        public TransactionPayload(byte[] bytes)
        {
            Version = BitConverter.ToInt32(bytes, 0);
            var remaining = bytes.Skip(4);

            var txInCount = new IntegerPayload(remaining.ToArray());
            remaining = remaining.Skip(txInCount.ToBytes().Length);

            TxInputs = new List<TxInputPayload>();
            for (UInt64 i = 0; i < txInCount.Integer; i++)
            {
                var txInput = new TxInputPayload(remaining.ToArray());
                remaining = remaining.Skip(txInput.ToBytes().Length);
                TxInputs.Add(txInput);
            }

            var txOutCount = new IntegerPayload(remaining.ToArray());
            remaining = remaining.Skip(txOutCount.ToBytes().Length);

            TxOutputs = new List<TxOutputPayload>();
            for (UInt64 i = 0; i < txOutCount.Integer; i++)
            {
                var txOutput = new TxOutputPayload(remaining.ToArray());
                remaining = remaining.Skip(txOutput.ToBytes().Length);
                TxOutputs.Add(txOutput);
            }

            LockTime = BitConverter.ToUInt32(remaining.ToArray(), 0);
        }

        public override byte[] ToBytes()
        {
            var bytes = new Byte[0].AsEnumerable();

            bytes = bytes.Concat(BitConverter.GetBytes(Version));

            var txInCount = new IntegerPayload((UInt64)TxInputs.Count);
            bytes = bytes.Concat(txInCount.ToBytes());
            foreach (TxInputPayload input in TxInputs)
            {
                bytes = bytes.Concat(input.ToBytes());
            }

            var txOutCount = new IntegerPayload((UInt64)TxOutputs.Count);
            bytes = bytes.Concat(txOutCount.ToBytes());
            foreach (TxOutputPayload output in TxOutputs)
            {
                bytes = bytes.Concat(output.ToBytes());
            }

            bytes = bytes.Concat(BitConverter.GetBytes(LockTime));

            return bytes.ToArray();
        }
    }
}
