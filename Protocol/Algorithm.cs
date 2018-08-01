using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Numerics;

namespace Protocol
{
    public static class Algorithm
    {
        public class TargetChainLengthTooBig : Exception { }
        public class TargetChainLengthTooSmall : Exception { }
        public class BlockHeaderHashTooSmall : Exception { }
        public class ChainOriginTooBig : Exception { }
        public class ChainOriginTooSmall : Exception { }

        public enum PrimeChainType
        {
            CunnighamFirstKind = 1,
            CunnighamSecondKind,
            BiTwin,
        };

        public static List<UInt32> GeneratePrimes() {
            return new List<UInt32> {
                  2,   3,   5,   7,  11,  13,  17,  19,  23,  29,
                 31,  37,  41,  43,  47,  53,  59,  61,  67,  71,
                 73,  79,  83,  89,  97, 101, 103, 107, 109, 113,
                127, 131, 137, 139, 149, 151, 157, 163, 167, 173,
                179, 181, 191, 193, 197, 199, 211, 223, 227, 229,
                233, 239, 241, 251, 257, 263, 269, 271, 277, 281,
                283, 293, 307, 311, 313, 317, 331, 337, 347, 349,
                353, 359, 367, 373, 379, 383, 389, 397, 401, 409,
                419, 421, 431, 433, 439, 443, 449, 457, 461, 463,
                467, 479, 487, 491, 499, 503, 509, 521, 523, 541,
                547, 557, 563, 569, 571, 577, 587, 593, 599, 601,
                607, 613, 617, 619, 631, 641, 643, 647, 653, 659,
                661, 673, 677, 683, 691, 701, 709, 719, 727, 733,
                739, 743, 751, 757, 761, 769, 773, 787, 797, 809,
                811, 821, 823, 827, 829, 839, 853, 857, 859, 863,
                877, 881, 883, 887, 907, 911, 919, 929, 937, 941,
                947, 953, 967, 971, 977, 983, 991, 997
            };
        }

        public static List<UInt32> PrimeNumbers = GeneratePrimes();

        public static Byte[] MerkleRoot(List<TransactionPayload> txs)
        {
            SHA256 sha256 = SHA256Managed.Create();

            var hashes = new List<Byte[]>();
            foreach (TransactionPayload tx in txs)
            {
                hashes.Add(sha256.ComputeHash(sha256.ComputeHash(tx.ToBytes())));
            }

            while (hashes.Count > 1)
            {
                // Ensure that the "row" of the merkle tree has an even number of elements.
                if (hashes.Count % 2 != 0)
                {
                    hashes.Add(hashes[hashes.Count - 1]);
                }
                var combinedHashes = new List<Byte[]>();
                for (Int32 i = 0; i < hashes.Count; i += 2)
                {
                    var combined = hashes[i].Concat(hashes[i + 1]).ToArray();
                    combinedHashes.Add(sha256.ComputeHash(sha256.ComputeHash(combined)));
                }
                hashes = combinedHashes;
            }

            return hashes[0];
        }

        public static DifficultyPayload FermatProbablePrimalityTest(BigInteger num)
        {
            var a = new BigInteger(2);
            var e = num - 1;
            var r = BigInteger.ModPow(a, e, num);
            if (r == 1)
            {
                return new DifficultyPayload(length: 1, fraction: 0);
            }
            var fraction = ((num - r) << 24) / num;
            return new DifficultyPayload(length: 0, fraction: (UInt32)fraction);
        }

        public static DifficultyPayload EulerLagrangeLifchitzPrimalityTest(BigInteger num, Boolean useSophieGermain)
        {
            var a = new BigInteger(2);
            var e = (num - 1) >> 1;
            var r = BigInteger.ModPow(a, e, num);
            var numMod8 = num % 8;

            Boolean isProbablePrime;
            if (useSophieGermain && (numMod8 == 7)) // Euler & Lagrange
            {
                isProbablePrime = (r == 1);
            }
            else if (useSophieGermain && (numMod8 == 3)) // Lifchitz
            {
                isProbablePrime = ((r + 1) == num);
            }
            else if (!useSophieGermain && (numMod8 == 5)) // Lifchitz
            {
                isProbablePrime = ((r + 1) == num);
            }
            else if (!useSophieGermain && (numMod8 == 1)) // Lifchitz
            {
                isProbablePrime = (r == 1);
            }
            else
            {
                throw new Exception("EulerLagrangeLifchitzPrimalityTest invalid blah blah blah.");
            }

            if (isProbablePrime)
            {
                return new DifficultyPayload(1, 0);
            }
            r = (r * r) % num; // derive Fermat test remainder
            var fraction = ((num - r) << 24) / num;
            return new DifficultyPayload(0, (UInt32)fraction);
        }

        public static DifficultyPayload ProbableCunninghamChainTest(
            BigInteger num,
            Boolean useSophieGermain,
            Boolean useAllFermat
        )
        {
            Byte chainLength = 0;
            DifficultyPayload result = FermatProbablePrimalityTest(num);
            if (result.Length != 1)
            {
                return new DifficultyPayload(length: chainLength, fraction: result.Fraction);
            }
            chainLength++;

            var offset = useSophieGermain ? 1 : -1;
            while (true)
            {
                num = 2 * num + offset;
                result = useAllFermat ? FermatProbablePrimalityTest(num)
                                      : EulerLagrangeLifchitzPrimalityTest(num, useSophieGermain);
                if (result.Length != 1)
                {
                    return new DifficultyPayload(length: chainLength, fraction: result.Fraction);
                }
                chainLength++;
            }
        }

        public static
        Tuple<Boolean, DifficultyPayload, DifficultyPayload, DifficultyPayload>
        ProbablePrimeChainTest(BigInteger primeChainOrigin, DifficultyPayload target, Boolean useAllFermatTests)
        {
            var cunninghamOne = ProbableCunninghamChainTest(primeChainOrigin - 1, true, useAllFermatTests);
            var cunninghamTwo = ProbableCunninghamChainTest(primeChainOrigin + 1, false, useAllFermatTests);
            DifficultyPayload biTwin;
            if (cunninghamOne.Length > cunninghamTwo.Length)
            {
                biTwin = new DifficultyPayload(
                    length: (Byte)(cunninghamTwo.Length + cunninghamTwo.Length + 1),
                    fraction: cunninghamTwo.Fraction
                );
            }
            else
            {
                biTwin = new DifficultyPayload(
                    length: (Byte)(cunninghamOne.Length + cunninghamOne.Length),
                    fraction: cunninghamOne.Fraction
                );
            }

            Boolean passesTargetDifficulty = (
                cunninghamOne > target
                || cunninghamTwo > target
                || biTwin > target
            );
            return Tuple.Create(passesTargetDifficulty, cunninghamOne, cunninghamTwo, biTwin);
        }

        public static
        Tuple<PrimeChainType, DifficultyPayload>
        CheckProofOfWork(BlockPayload block, ProtocolConfiguration protocolConfig)
        {
            var difficulty = new DifficultyPayload(block.Bits);
            if (difficulty.Length < protocolConfig.MinimumChainLength)
            {
                throw new TargetChainLengthTooSmall();
            }
            if (difficulty.Length > protocolConfig.MaximumChainLength)
            {
                throw new TargetChainLengthTooBig();
            }

            var headerHash = block.HeaderHash();
            if (headerHash < protocolConfig.MinimumHeaderHash)
            {
                throw new BlockHeaderHashTooSmall();
            }

            var chainOrigin = headerHash * block.PrimeChainMultiplier;
            if (chainOrigin < protocolConfig.MinimumChainOrigin)
            {
                throw new ChainOriginTooSmall();
            }
            if (chainOrigin > protocolConfig.MaximumChainOrigin)
            {
                throw new ChainOriginTooBig();
            }

            var result = ProbablePrimeChainTest(chainOrigin, difficulty, false);
            if (result.Item1 == false)
            {
                throw new Exception("chainOrigin failed ProbablePrimeChainTest");
            }
            if (result.Item2 < difficulty && result.Item3 < difficulty && result.Item4 < difficulty)
            {
                throw new Exception("chain difficulty too small");
            }

            var fermatResult = ProbablePrimeChainTest(chainOrigin, difficulty, true);
            if (fermatResult.Item1 == false)
            {
                throw new Exception("chainOrigin failed ProbablePrimeChainTest fermat tests");
            }
            if (
                fermatResult.Item2.Length != result.Item2.Length
             || fermatResult.Item2.Fraction != result.Item2.Fraction
             || fermatResult.Item3.Length != result.Item3.Length
             || fermatResult.Item3.Fraction != result.Item3.Fraction
             || fermatResult.Item4.Length != result.Item4.Length
             || fermatResult.Item4.Fraction != result.Item4.Fraction
            )
            {
                throw new Exception("chain difficulty mismatch on second pass");
            }

            // Determine the best prime chain.
            var chainType = PrimeChainType.CunnighamFirstKind;
            var chainDifficulty = result.Item2;
            if (result.Item3 > chainDifficulty)
            {
                chainType = PrimeChainType.CunnighamSecondKind;
                chainDifficulty = result.Item3;
            }
            if (result.Item4 > chainDifficulty)
            {
                chainType = PrimeChainType.BiTwin;
                chainDifficulty = result.Item4;
            }

            // Check that PCM is "normalized".
            if (block.PrimeChainMultiplier % 2 == 0 && chainOrigin % 4 == 0)
            {
                var normalizedResult = ProbablePrimeChainTest(chainOrigin / 2, difficulty, false);
                if (normalizedResult.Item1 == true)
                {
                    if (
                        normalizedResult.Item2 > chainDifficulty
                     || normalizedResult.Item3 > chainDifficulty
                     || normalizedResult.Item4 > chainDifficulty
                    )
                    {
                        throw new Exception("PCM is not normalized");
                    }
                }
            }

            return Tuple.Create(chainType, chainDifficulty);
        }

        public static Boolean ProbablePrimalityTestWithTrialDivision(BigInteger candidate)
        {
            foreach (UInt32 primeNumber in PrimeNumbers)
            {
                if (candidate % primeNumber == 0)
                {
                    return false;
                }
            }

            var result = FermatProbablePrimalityTest(candidate);
            return result.Length == 1;
        }

        public static BigInteger Primorial(UInt32 p)
        {
            var primorial = new BigInteger(1);
            foreach(UInt32 prime in PrimeNumbers)
            {
                if (prime > p)
                {
                    return primorial;
                }
                primorial *= prime;
            }
            throw new Exception("Exhausted PrimeNumbers list computing p#");
        }

        public static UInt64 BlockReward(DifficultyPayload difficulty)
        {
            const UInt64 COIN = 100000000;
            const UInt64 CENT = 1000000;
            const Int32 FRACTION_BITS = 24;

            var reward = new BigInteger(999 * COIN);
            reward = (reward << FRACTION_BITS) / difficulty.ToBits();
            reward = (reward << FRACTION_BITS) / difficulty.ToBits();
            reward = (reward / CENT) * CENT;
            return (UInt64)reward;
        }

        public const Int64 TARGET_TIMESPAN = 7 * 24 * 60 * 60; // one week
        public const Int64 TARGET_SPACING = 60; // one minute block spacing
        public const Int64 TARGET_INTERVAL = TARGET_TIMESPAN / TARGET_SPACING;

        public const Int32 FRACTION_BITS = 24;
        public const UInt32 FRACTION_MASK = ((UInt32)1 << FRACTION_BITS) - 1;
        public const UInt32 LENGTH_MASK = ~FRACTION_MASK;
        public const UInt64 FRACTION_MAX = (UInt64)1 << (FRACTION_BITS + 32);
        public const UInt64 FRACTION_MIN = (UInt64)1 << 32;
        public const UInt64 FRACTION_THRESHOLD = (UInt64)1 << (8 + 32);

        public static DifficultyPayload NextDifficulty(DifficultyPayload difficulty, Int64 blockSpacing)
        {
            var fraction = new BigInteger(difficulty.GetFractionalDifficulty());
            fraction *= ((TARGET_INTERVAL + 1) * TARGET_SPACING);
            fraction /= ((TARGET_INTERVAL - 1) * TARGET_SPACING + blockSpacing + blockSpacing);
            if (fraction > FRACTION_MAX)
            {
                fraction = FRACTION_MAX;
            }
            else if (fraction < FRACTION_MIN)
            {
                fraction = FRACTION_MIN;
            }
            
            UInt64 newFraction = (UInt64)fraction;
            if (newFraction > FRACTION_THRESHOLD)
            {
                difficulty.Length++;
                newFraction = FRACTION_MIN;
            }
            else if (newFraction == FRACTION_MIN)
            {
                difficulty.Length--;
                newFraction = FRACTION_THRESHOLD;
            }
            difficulty.SetFractionalDifficulty(newFraction);
            return difficulty;
        }
    }
}
