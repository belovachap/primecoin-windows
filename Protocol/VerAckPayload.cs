using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol
{
    public class VerAckPayload : Payload
    {
        public VerAckPayload() { }

        public VerAckPayload(byte[] bytes) { }

        public override byte[] ToBytes()
        {
            return new byte[] { };
        }
    }
}
