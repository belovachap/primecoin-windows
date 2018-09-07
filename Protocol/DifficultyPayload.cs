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

        public UInt64 GetFractionalDifficulty()
        {
            return Algorithm.FRACTION_MAX / (((UInt64)1 << Algorithm.FRACTION_BITS) - Fraction);
        }

        public void SetFractionalDifficulty(UInt64 fractionalDifficulty)
        {
            if (fractionalDifficulty < Algorithm.FRACTION_MIN)
            {
                throw new Exception("SetFractionalDifficulty() : difficulty below min");
            }
                
            UInt64 fraction = Algorithm.FRACTION_MAX / fractionalDifficulty;
            if (fraction > ((UInt32)1 << Algorithm.FRACTION_BITS))
            {
                throw new Exception("SetFractionalDifficulty() : fractional overflow");
            }
            Fraction = ((UInt32)1 << Algorithm.FRACTION_BITS) - (UInt32)fraction;
        }

        public DifficultyPayload(Byte length, UInt32 fraction)
        {
            Length = length;
            Fraction = fraction;
        }

        public DifficultyPayload(UInt32 bits)
        {
            Length = (Byte)(bits >> Algorithm.FRACTION_BITS);
            Fraction = bits & Algorithm.FRACTION_MASK;
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
