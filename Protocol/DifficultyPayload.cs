using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol
{
    public class DifficultyPayload : Payload
    {
        public Byte Length;
        public UInt32 Fraction;

        public DifficultyPayload(Byte length, UInt32 fraction)
        {
            Length = length;
            Fraction = fraction;
        }

        public DifficultyPayload(UInt32 bits)
        {
            var bytes = BitConverter.GetBytes(bits);
            Length = bytes[3];

            bytes[3] = 0x00;
            Fraction = BitConverter.ToUInt32(bytes, 0);
        }

        public UInt32 ToBits()
        {
            return BitConverter.ToUInt32(ToBytes(), 0);
        }

        public override Byte[] ToBytes()
        {
            var bytes = BitConverter.GetBytes(Fraction);
            bytes[3] = Length;
            return bytes;
        }

        public static bool operator >(DifficultyPayload left, DifficultyPayload right)
        {
            if (left.Length > right.Length)
            {
                return true;
            }
            else if (left.Length == right.Length && left.Fraction > right.Fraction)
            {
                return true;
            }

            return false;
        }

        public static bool operator <(DifficultyPayload left, DifficultyPayload right)
        {
            if (left.Length < right.Length)
            {
                return true;
            }
            else if (left.Length == right.Length && left.Fraction < right.Fraction)
            {
                return true;
            }

            return false;
        }
    }
}
