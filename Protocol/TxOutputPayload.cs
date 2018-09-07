using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol
{
    public class TxOutputPayload : Payload
    {

        public Int64 Amount { get; }
        public UnknownPayload Script { get; }

        public TxOutputPayload(Int64 amount, UnknownPayload script)
        {
            Amount = amount;
            Script = script;
        }

        public TxOutputPayload(byte[] bytes)
        {
            Amount = BitConverter.ToInt64(bytes, 0);
            var remaining = bytes.Skip(8);

            var scriptLength = new IntegerPayload(remaining.ToArray());
            remaining = remaining.Skip(scriptLength.ToBytes().Length);

            Script = new UnknownPayload(remaining.Take((Int32)scriptLength.Integer).ToArray());
        }

        public override byte[] ToBytes()
        {
            var amount = BitConverter.GetBytes(Amount);
            var script = Script.ToBytes();
            var scriptLength = new IntegerPayload((UInt64)script.Length);

            return amount
                   .Concat(scriptLength.ToBytes())
                   .Concat(script)
                   .ToArray();
        }
    }
}
