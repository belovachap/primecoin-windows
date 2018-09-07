using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol
{
    public class InvEntryPayload : Payload
    {
        public enum InvEntryType
        {
            ERROR,
            MSG_TX,
            MSG_BLOCK,
            MSG_FILTERED_BLOCK,
            MSG_CMPCT_BLOCK
        };
        public InvEntryType Type { get; }
        public Byte[] Hash { get; }

        Dictionary<UInt32, InvEntryType> InvEntryValueToType = new Dictionary<UInt32, InvEntryType> {
            { 0, InvEntryType.ERROR },
            { 1, InvEntryType.MSG_TX },
            { 2, InvEntryType.MSG_BLOCK },
            { 3, InvEntryType.MSG_FILTERED_BLOCK },
            { 4, InvEntryType.MSG_CMPCT_BLOCK },
        };
        Dictionary<InvEntryType, UInt32> TypeToInvEntryValue = new Dictionary<InvEntryType, UInt32> {
            { InvEntryType.ERROR, 0 },
            { InvEntryType.MSG_TX, 1 },
            { InvEntryType.MSG_BLOCK, 2 },
            { InvEntryType.MSG_FILTERED_BLOCK, 3 },
            { InvEntryType.MSG_CMPCT_BLOCK, 4 },
        };

        public InvEntryPayload(InvEntryType type, Byte[] hash)
        {
            Type = type;
            Hash = hash;
        }

        public InvEntryPayload(Byte[] bytes)
        {
            var invEntryValue = BitConverter.ToUInt32(bytes, 0);
            Type = InvEntryValueToType[invEntryValue];
            Hash = bytes.Skip(4).Take(32).ToArray();
        }

        public override Byte[] ToBytes()
        {
            var typeBytes = BitConverter.GetBytes(TypeToInvEntryValue[Type]);
            return typeBytes.Concat(Hash).ToArray();
        }
    }
}
