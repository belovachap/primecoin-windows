using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol
{
    public class TxInputPayload : Payload
    {
        public TxOutPointPayload PreviousTransaction { get; }
        public UnknownPayload Script { get; }
        public UInt32 Sequence { get; }

        public TxInputPayload(
            TxOutPointPayload previousTx,
            UnknownPayload script,
            UInt32 sequence
        )
        {
            PreviousTransaction = previousTx;
            Script = script;
            Sequence = sequence;
        }

        public TxInputPayload(byte[] bytes)
        {
            PreviousTransaction = new TxOutPointPayload(bytes);
            var remaining = bytes.Skip(PreviousTransaction.ToBytes().Length);

            var scriptLength = new IntegerPayload(remaining.ToArray());
            remaining = remaining.Skip(scriptLength.ToBytes().Length);

            Script = new UnknownPayload(remaining.Take((Int32)scriptLength.Integer).ToArray());
            remaining = remaining.Skip(Script.ToBytes().Length);

            Sequence = BitConverter.ToUInt32(remaining.ToArray(), 0);
        }

        public override byte[] ToBytes()
        {
            var txOutPointBytes = PreviousTransaction.ToBytes();
            var scriptBytes = Script.ToBytes();
            var scriptLengthBytes = new IntegerPayload((UInt64)scriptBytes.Length).ToBytes();
            var sequenceBytes = BitConverter.GetBytes(Sequence);

            return txOutPointBytes
                   .Concat(scriptLengthBytes)
                   .Concat(scriptBytes)
                   .Concat(sequenceBytes)
                   .ToArray();
        }
    }
}
