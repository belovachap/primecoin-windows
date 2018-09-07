using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol
{
    public class UnknownPayload : Payload
    {
        public Byte[] Bytes { get; }

        public UnknownPayload(Byte[] bytes)
        {
            Bytes = bytes;
        }

        public override byte[] ToBytes()
        {
            return Bytes;
        }
    }
}
