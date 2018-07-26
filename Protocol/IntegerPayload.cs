using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol
{
    public class IntegerPayload : Payload
    {
        public UInt64 Integer { get; }

        public IntegerPayload(UInt64 integer)
        {
            Integer = integer;
        }

        public IntegerPayload(Byte[] bytes)
        {
            if (bytes.Length == 0)
            {
                throw new ArgumentException("IntegerPayload from bytes requires at least one byte.");
            }

            if (bytes[0] < 0xFD)
            {
                Integer = (UInt64)bytes[0];
            }
            else if (bytes[0] == 0xFD)
            {
                Integer = (UInt64)BitConverter.ToUInt16(bytes, 1);
            }
            else if (bytes[0] == 0xFE)
            {
                Integer = (UInt64)BitConverter.ToUInt32(bytes, 1);
            }
            else
            {
                Integer = BitConverter.ToUInt64(bytes, 1);
            }
        }

        public override byte[] ToBytes()
        {
            byte[] magicByte;
            byte[] integerBytes;

            if (Integer < 0xFD)
            {
                return new byte[] { (byte)Integer };
            }
            else if (Integer <= 0xFFFF)
            {
                magicByte = new byte[] { 0xFD };
                integerBytes = BitConverter.GetBytes((UInt16)Integer);
                return magicByte.Concat(integerBytes).ToArray();
            }
            else if (Integer <= 0xFFFFFFFF)
            {
                magicByte = new byte[] { 0xFE };
                integerBytes = BitConverter.GetBytes((UInt32)Integer);
                return magicByte.Concat(integerBytes).ToArray();
            }
            else
            {
                magicByte = new byte[] { 0xFF };
                integerBytes = BitConverter.GetBytes(Integer);
                return magicByte.Concat(integerBytes).ToArray();
            }
        }
    }
}
