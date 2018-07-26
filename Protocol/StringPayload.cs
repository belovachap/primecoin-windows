using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol
{
    public class StringPayload : Payload
    {
        public String String { get; }

        public StringPayload(String str)
        {
            String = str;
        }

        public StringPayload(Byte[] bytes)
        {
            if (bytes.Length == 0)
            {
                throw new ArgumentException("StringPayload from bytes requires at least one byte.");
            }

            if (bytes[0] == 0x00)
            {
                String = "";
            }
            else
            {
                var size = new IntegerPayload(bytes);
                var offset = size.ToBytes().Length;
                String = Encoding.ASCII.GetString(bytes, offset, (Int32)size.Integer);
            }
        }

        public override byte[] ToBytes()
        {
            var strBytes = Encoding.ASCII.GetBytes(String);
            var sizeBytes = new IntegerPayload((UInt64)strBytes.Length).ToBytes();
            return sizeBytes.Concat(strBytes).ToArray();
        }
    }
}
