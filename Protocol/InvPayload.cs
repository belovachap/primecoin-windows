using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol
{
    public class InvPayload : Payload
    {
        public List<InvEntryPayload> Entries { get; }

        public InvPayload(List<InvEntryPayload> entries)
        {
            Entries = entries;
        }

        public InvPayload(byte[] bytes)
        {
            var count = new IntegerPayload(bytes);
            var remaining = bytes.Skip(count.ToBytes().Length);

            Entries = new List<InvEntryPayload>();
            for (UInt64 i = 0; i < count.Integer; i++)
            {
                var invEntry = new InvEntryPayload(remaining.ToArray());
                Entries.Add(invEntry);
                remaining = remaining.Skip(invEntry.ToBytes().Length);
            }
        }

        public override Byte[] ToBytes()
        {
            var count = new IntegerPayload((UInt64)Entries.Count);
            var bytes = count.ToBytes().AsEnumerable();

            for (Int32 i = 0; i < Entries.Count; i++)
            {
                bytes = bytes.Concat(Entries[i].ToBytes());
            }

            return bytes.ToArray();
        }
    }
}
