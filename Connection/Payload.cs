using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Security.Cryptography;
using System.Numerics;


namespace Connection
{
    public abstract class Payload
    {
        public abstract byte[] ToBytes();
    }

    public class MessagePayload : Payload
    {
        public UInt32 Magic { get; }
        public String Command { get; }
        public Payload CommandPayload { get; }

        public MessagePayload(UInt32 magic, String command, Payload commandPayload)
        {
            if (Encoding.ASCII.GetBytes(command).Length > 12)
            {
                throw new ArgumentException("command must be 12 or fewer ascii bytes");
            }
            Magic = magic;
            Command = command;
            CommandPayload = commandPayload;
        }

        public MessagePayload(byte[] bytes)
        {
            Magic = BitConverter.ToUInt32(bytes, 0);
            var remaining = bytes.Skip(4);

            Command = Encoding.ASCII.GetString(remaining.Take(12).ToArray());
            Command = Command.TrimEnd(new char[] { '\0' });
            remaining = remaining.Skip(12);

            var length = BitConverter.ToUInt32(remaining.ToArray(), 0);
            remaining = remaining.Skip(4);

            var checkSum = remaining.Take(4).ToArray();
            remaining = remaining.Skip(4);

            var payload = remaining.Take((Int32)length).ToArray();

            if (payload.Length != length)
            {
                throw new ArgumentException("payload was not of expected length!");
            }

            SHA256 sha256 = SHA256Managed.Create();
            var computedCheckSum = sha256.ComputeHash(sha256.ComputeHash(payload)).Take(4);

            if (!checkSum.SequenceEqual(computedCheckSum.ToArray()))
            {
                throw new ArgumentException("checksum of payload does not match!");
            }

            switch (Command)
            {
                case "block":
                    CommandPayload = new BlockPayload(payload);
                    break;

                case "inv":
                    CommandPayload = new InvPayload(payload);
                    break;

                case "verack":
                    CommandPayload = new VerAckPayload(payload);
                    break;

                case "version":
                    CommandPayload = new VersionPayload(payload);
                    break;

                default:
                    CommandPayload = new UnknownPayload(payload);
                    break;
            }
        }

        public override byte[] ToBytes()
        {
            var magicBytes = BitConverter.GetBytes(Magic);

            var paddedCommandBytes = new Byte[12];
            var commandBytes = Encoding.ASCII.GetBytes(Command);
            for (Int32 i = 0; i < commandBytes.Length; i++)
            {
                paddedCommandBytes[i] = commandBytes[i];
            }

            var payloadBytes = CommandPayload.ToBytes();
            var lengthBytes = BitConverter.GetBytes((UInt32)payloadBytes.Length);

            SHA256 sha256 = SHA256Managed.Create();
            var checkSumBytes = sha256.ComputeHash(sha256.ComputeHash(payloadBytes)).Take(4);

            return magicBytes
                   .Concat(paddedCommandBytes)
                   .Concat(lengthBytes)
                   .Concat(checkSumBytes)
                   .Concat(payloadBytes)
                   .ToArray();
        }
    }

    public class IntegerPayload : Payload
    {
        public UInt64 Integer { get; }

        public IntegerPayload(UInt64 integer)
        {
            Integer = integer;
        }

        public IntegerPayload(Byte[] bytes)
        {
            if (bytes.Length == 0) {
                throw new ArgumentException("IntegerPayload from bytes requires at least one byte.");
            }

            if (bytes[0] < 0xFD) {
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

    public class IPAddressPayload : Payload
    {
        public DateTime TimeStamp { get; }
        public UInt64 Services { get; }
        public IPAddress Address { get; }
        public UInt16 Port { get; }

        public IPAddressPayload(
            DateTime timeStamp,
            UInt64 services,
            IPAddress address,
            UInt16 port
        )
        {
            TimeStamp = timeStamp;
            Services = services;
            Address = address;
            Port = port;
        }

        public IPAddressPayload(byte[] bytes)
        {
            TimeStamp = new DateTime(BitConverter.ToUInt32(bytes, 0));
            Services = BitConverter.ToUInt64(bytes, 4);

            var ipBytes = bytes.Skip(12).Take(16).ToArray();
            var paddingBytes = new byte[] {
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0xFF, 0xFF,
            };
            // Check for an IPv4 address.
            if (ipBytes.Take(12).SequenceEqual(paddingBytes))
            {
                Address = new IPAddress(ipBytes.Skip(12).ToArray());
            }
            else
            {
                Address = new IPAddress(ipBytes);
            }

            // Port sent in "Network Order", reverse the bytes for BitConverter.
            var portBytes = new byte[] { bytes[29], bytes[28] };
            Port = BitConverter.ToUInt16(portBytes, 0);
        }

        public override byte[] ToBytes()
        {
            byte[] timeStampBytes = BitConverter.GetBytes((UInt32)TimeStamp.Ticks);
            byte[] servicesBytes = BitConverter.GetBytes(Services);
            byte[] addressBytes = IPAddressToBytes(Address);
            byte[] portBytes = BitConverter.GetBytes(Port);
            Array.Reverse(portBytes); // Sent in network byte order

            return timeStampBytes
                   .Concat(servicesBytes)
                   .Concat(addressBytes)
                   .Concat(portBytes)
                   .ToArray();
        }

        public static byte[] IPAddressToBytes(IPAddress address)
        {
            switch (address.GetAddressBytes().Length)
            {
                case 4:
                    var padding_bytes = new byte[]
                    {
                        0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0xFF, 0xFF,
                    };
                    return padding_bytes
                           .Concat(address.GetAddressBytes())
                           .ToArray();
                case 16:
                    return address.GetAddressBytes();
                default:
                    throw new Exception("Unexpected number of IPAddress bytes!");
            }
        }
    }

    public class VersionPayload : Payload
    {
        public Int32 Version { get; }
        public UInt64 Services { get; }
        public DateTime TimeStamp { get; }
        public IPAddressPayload AddressTo { get; }
        public IPAddressPayload AddressFrom { get; }
        public UInt64 Nonce { get; }
        public StringPayload UserAgent { get; }
        public UInt32 StartHeight { get; }
        public Boolean Relay { get; }

        public VersionPayload(
            Int32 version,
            UInt64 services,
            DateTime timeStamp,
            IPAddressPayload addressTo,
            IPAddressPayload addressFrom,
            UInt64 nonce,
            StringPayload userAgent,
            UInt32 startHeight,
            Boolean relay
        )
        {
            Version = version;
            Services = services;
            TimeStamp = timeStamp;
            AddressTo = addressTo;
            AddressFrom = addressFrom;
            Nonce = nonce;
            UserAgent = userAgent;
            StartHeight = startHeight;
            Relay = relay;
        }

        public VersionPayload(byte[] bytes)
        {
            Version = BitConverter.ToInt32(bytes, 0);
            Services = BitConverter.ToUInt64(bytes, 4);
            TimeStamp = new DateTime(BitConverter.ToInt64(bytes, 12));
            var remaining = bytes.Skip(20);

            // AddressTo
            var missingTimeStampBytes = new byte[] {
                0x00, 0x00, 0x00, 0x00,
            };
            var addressToBytes = missingTimeStampBytes
                                 .Concat(remaining.Take(26))
                                 .ToArray();
            AddressTo = new IPAddressPayload(addressToBytes);
            remaining = remaining.Skip(26);

            // AddressFrom
            var addressFromBytes = missingTimeStampBytes
                                   .Concat(remaining.Take(26))
                                   .ToArray();
            AddressFrom = new IPAddressPayload(addressFromBytes);
            remaining = remaining.Skip(26);

            Nonce = BitConverter.ToUInt64(remaining.ToArray(), 0);
            remaining = remaining.Skip(8);

            UserAgent = new StringPayload(remaining.ToArray());
            remaining = remaining.Skip(UserAgent.ToBytes().Length);

            StartHeight = BitConverter.ToUInt32(remaining.ToArray(), 0);
            remaining = remaining.Skip(4);

            if (remaining.Count() != 0)
            {
                Relay = BitConverter.ToBoolean(remaining.ToArray(), 0);
            }
        }

        public override byte[] ToBytes()
        {
            var versionBytes = BitConverter.GetBytes(Version);
            var servicesBytes = BitConverter.GetBytes(Services);
            var timeStampBytes = BitConverter.GetBytes(TimeStamp.Ticks);

            // We leave out the IPAddress TimeStamp in the Version Message.
            var addressToBytes = AddressTo.ToBytes().Skip(4).ToArray();
            var addressFromBytes = AddressFrom.ToBytes().Skip(4).ToArray();

            var nonceBytes = BitConverter.GetBytes(Nonce);
            var userAgentBytes = UserAgent.ToBytes();
            var startHeightBytes = BitConverter.GetBytes(StartHeight);
            var relayBytes = BitConverter.GetBytes(Relay);

            return versionBytes
                   .Concat(servicesBytes)
                   .Concat(timeStampBytes)
                   .Concat(addressToBytes)
                   .Concat(addressFromBytes)
                   .Concat(nonceBytes)
                   .Concat(userAgentBytes)
                   .Concat(startHeightBytes)
                   .Concat(relayBytes)
                   .ToArray();
        }
    }

    public class VerAckPayload : Payload
    {
        public VerAckPayload() { }

        public VerAckPayload(byte[] bytes) { }

        public override byte[] ToBytes()
        {
            return new byte[] { };
        }
    }

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

    public enum InvEntryType {
        ERROR,
        MSG_TX,
        MSG_BLOCK,
        MSG_FILTERED_BLOCK,
        MSG_CMPCT_BLOCK
    };

    public class InvEntryPayload : Payload
    {
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

    public class TransactionPayload : Payload
    {

        public Int32 Version { get; }
        public List<TxInputPayload> TxInputs;
        public List<TxOutputPayload> TxOutputs;
        public UInt32 LockTime;

        public TransactionPayload(
            Int32 version,
            List<TxInputPayload> txInputs,
            List<TxOutputPayload> txOutputs,
            UInt32 lockTime
        )
        {
            Version = version;
            TxInputs = txInputs;
            TxOutputs = txOutputs;
            LockTime = lockTime;
        }

        public TransactionPayload(byte[] bytes)
        {
            Version = BitConverter.ToInt32(bytes, 0);
            var remaining = bytes.Skip(4);

            var txInCount = new IntegerPayload(remaining.ToArray());
            remaining = remaining.Skip(txInCount.ToBytes().Length);

            TxInputs = new List<TxInputPayload>();
            for (UInt64 i = 0; i < txInCount.Integer; i++)
            {
                var txInput = new TxInputPayload(remaining.ToArray());
                remaining = remaining.Skip(txInput.ToBytes().Length);
                TxInputs.Add(txInput);
            }

            var txOutCount = new IntegerPayload(remaining.ToArray());
            remaining = remaining.Skip(txOutCount.ToBytes().Length);

            TxOutputs = new List<TxOutputPayload>();
            for (UInt64 i = 0; i < txOutCount.Integer; i++)
            {
                var txOutput = new TxOutputPayload(remaining.ToArray());
                remaining = remaining.Skip(txOutput.ToBytes().Length);
                TxOutputs.Add(txOutput);
            }

            LockTime = BitConverter.ToUInt32(remaining.ToArray(), 0);
        }

        public override byte[] ToBytes()
        {
            var bytes = new Byte[0].AsEnumerable();

            bytes = bytes.Concat(BitConverter.GetBytes(Version));

            var txInCount = new IntegerPayload((UInt64)TxInputs.Count);
            bytes = bytes.Concat(txInCount.ToBytes());
            foreach (TxInputPayload input in TxInputs)
            {
                bytes = bytes.Concat(input.ToBytes());
            }

            var txOutCount = new IntegerPayload((UInt64)TxOutputs.Count);
            bytes = bytes.Concat(txOutCount.ToBytes());
            foreach (TxOutputPayload output in TxOutputs)
            {
                bytes = bytes.Concat(output.ToBytes());
            }

            bytes = bytes.Concat(BitConverter.GetBytes(LockTime));

            return bytes.ToArray();
        }
    }

    public class TargetChainLengthTooBig : Exception { }
    public class TargetChainLengthTooSmall : Exception { }
    public class BlockHeaderHashTooSmall : Exception { }
    public class ChainOriginTooBig : Exception { }
    public class ChainOriginTooSmall : Exception { }

    public class Difficulty
    {
        public Byte Length;
        public UInt32 Fraction;

        public Difficulty(Byte length, UInt32 fraction)
        {
            Length = length;
            Fraction = fraction;
        }

        public Difficulty(UInt32 bits)
        {
            var bytes = BitConverter.GetBytes(bits);
            Length = bytes[3];

            bytes[3] = 0x00;
            Fraction = BitConverter.ToUInt32(bytes, 0);
        }

        public UInt32 ToBits()
        {
            var bytes = BitConverter.GetBytes(Fraction);
            bytes[3] = Length;
            return BitConverter.ToUInt32(bytes, 0);
        }

        public static bool operator >(Difficulty left, Difficulty right)
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

        public static bool operator <(Difficulty left, Difficulty right)
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

    // Pulling out some functions for easier testing and future moving around.
    public static class Algorithms
    {
        public enum PrimeChainType
        {
            CunnighamFirstKind = 1,
            CunnighamSecondKind,
            BiTwin,
        };

        public static List<UInt32> PrimeNumbers = new List<UInt32> {
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

        public static Difficulty FermatProbablePrimalityTest(BigInteger num)
        {
            var a = new BigInteger(2);
            var e = num - 1;
            var r = BigInteger.ModPow(a, e, num);
            if (r == 1)
            {
                return new Difficulty(length: 1, fraction: 0);
            }
            var fraction = ((num - r) << 24) / num;
            return new Difficulty(length: 0, fraction: (UInt32)fraction);
        }

        public static Difficulty EulerLagrangeLifchitzPrimalityTest(BigInteger num, Boolean useSophieGermain)
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
                return new Difficulty(1, 0);
            }
            r = (r * r) % num; // derive Fermat test remainder
            var fraction = ((num - r) << 24) / num;
            return new Difficulty(0, (UInt32)fraction);
        }


        public static Difficulty ProbableCunninghamChainTest(
            BigInteger num,
            Boolean useSophieGermain,
            Boolean useAllFermat
        )
        {
            Byte chainLength = 0;
            Difficulty result = FermatProbablePrimalityTest(num);
            if (result.Length != 1)
            {
                return new Difficulty(length: chainLength, fraction: result.Fraction);
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
                    return new Difficulty(length: chainLength, fraction: result.Fraction);
                }
                chainLength++;
            }
        }

        public static
        Tuple<Boolean, Difficulty, Difficulty, Difficulty>
        ProbablePrimeChainTest(BigInteger primeChainOrigin, Difficulty target, Boolean useAllFermatTests)
        {
            var cunninghamOne = ProbableCunninghamChainTest(primeChainOrigin - 1, true, useAllFermatTests);
            var cunninghamTwo = ProbableCunninghamChainTest(primeChainOrigin + 1, false, useAllFermatTests);
            Difficulty biTwin;
            if (cunninghamOne.Length > cunninghamTwo.Length)
            {
                biTwin = new Difficulty(
                    length: (Byte)(cunninghamTwo.Length + cunninghamTwo.Length + 1),
                    fraction: cunninghamTwo.Fraction
                );
            }
            else
            {
                biTwin = new Difficulty(
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
        Tuple<Algorithms.PrimeChainType, Difficulty>
        CheckProofOfWork(BlockPayload block, NetworkConfiguration networkConfig)
        {
            var difficulty = new Difficulty(block.Bits);
            if (difficulty.Length < networkConfig.MinimumChainLength)
            {
                throw new TargetChainLengthTooSmall();
            }
            if (difficulty.Length > networkConfig.MaximumChainLength)
            {
                throw new TargetChainLengthTooBig();
            }

            var headerHashBytes = block.HeaderHash().AsEnumerable();
            headerHashBytes = headerHashBytes.Concat(new Byte[] { 0x00 });
            var headerHash = new BigInteger(headerHashBytes.ToArray());
            if (headerHash < networkConfig.MinimumHeaderHash)
            {
                throw new BlockHeaderHashTooSmall();
            }

            var chainOrigin = headerHash * block.PrimeChainMultiplier;
            if (chainOrigin < networkConfig.MinimumChainOrigin)
            {
                throw new ChainOriginTooSmall();
            }
            if (chainOrigin > networkConfig.MaximumChainOrigin)
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
            var chainType = Algorithms.PrimeChainType.CunnighamFirstKind;
            var chainDifficulty = result.Item2;
            if (result.Item3 > chainDifficulty)
            {
                chainType = Algorithms.PrimeChainType.CunnighamSecondKind;
                chainDifficulty = result.Item3;
            }
            if (result.Item4 > chainDifficulty)
            {
                chainType = Algorithms.PrimeChainType.BiTwin;
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

        public static BigInteger Primorial()
        {
            return new BigInteger();
        }
    }

    public class BlockPayload : Payload
    {

        public Int32 Version { get; }
        public Byte[] PreviousBlockHash { get; }
        public UInt32 TimeStamp { get; }
        public UInt32 Bits { get; }
        public UInt32 Nonce;
        public BigInteger PrimeChainMultiplier;
        public List<TransactionPayload> Transactions { get; }
        public Byte[] MerkleRoot { get { return Algorithms.MerkleRoot(Transactions); } }

        public BlockPayload(
            Int32 version,
            Byte[] previousBlockHash,
            UInt32 timeStamp,
            UInt32 bits,
            UInt32 nonce,
            BigInteger primeChainMultiplier,
            List<TransactionPayload> txs
        )
        {
            Version = version;
            PreviousBlockHash = previousBlockHash;
            TimeStamp = timeStamp;
            Bits = bits;
            Nonce = nonce;
            PrimeChainMultiplier = primeChainMultiplier;
            Transactions = txs;
        }

        public BlockPayload(byte[] bytes)
        {
            Version = BitConverter.ToInt32(bytes, 0);
            var remaining = bytes.Skip(4);

            PreviousBlockHash = remaining.Take(32).ToArray();
            remaining = remaining.Skip(32);

            var merkleRoot = remaining.Take(32).ToArray();
            remaining = remaining.Skip(32);

            TimeStamp = BitConverter.ToUInt32(remaining.ToArray(), 0);
            remaining = remaining.Skip(4);

            Bits = BitConverter.ToUInt32(remaining.ToArray(), 0);
            remaining = remaining.Skip(4);

            Nonce = BitConverter.ToUInt32(remaining.ToArray(), 0);
            remaining = remaining.Skip(4);

            // Handle PrimeChainMultiplier...
            var pcmCount = new IntegerPayload(remaining.ToArray());
            remaining = remaining.Skip(pcmCount.ToBytes().Length);
            var pcmBytes = remaining.Take((Int32)pcmCount.Integer).ToArray();
            remaining = remaining.Skip(pcmBytes.Length);
            PrimeChainMultiplier = new BigInteger(pcmBytes);

            var txCount = new IntegerPayload(remaining.ToArray());
            remaining = remaining.Skip(txCount.ToBytes().Length);

            Transactions = new List<TransactionPayload>();
            for(UInt64 i = 0; i < txCount.Integer; i++)
            {
                var tx = new TransactionPayload(remaining.ToArray());
                remaining = remaining.Skip(tx.ToBytes().Length);
                Transactions.Add(tx);
            }

            if (!merkleRoot.SequenceEqual(MerkleRoot))
            {
                throw new Exception("MerkleRoot incorrect!");
            }
        }

        public override byte[] ToBytes()
        {
            var bytes = new Byte[0].AsEnumerable();
            bytes = bytes.Concat(BitConverter.GetBytes(Version));
            bytes = bytes.Concat(PreviousBlockHash);
            bytes = bytes.Concat(MerkleRoot);
            bytes = bytes.Concat(BitConverter.GetBytes(TimeStamp));
            bytes = bytes.Concat(BitConverter.GetBytes(Bits));
            bytes = bytes.Concat(BitConverter.GetBytes(Nonce));

            var pcmBytes = PrimeChainMultiplier.ToByteArray();
            var pcmCount = new IntegerPayload((UInt64)pcmBytes.Length);
            bytes = bytes.Concat(pcmCount.ToBytes()).Concat(pcmBytes);

            var txCount = new IntegerPayload((UInt64)Transactions.Count);
            bytes = bytes.Concat(txCount.ToBytes());
            foreach(TransactionPayload tx in Transactions)
            {
                bytes = bytes.Concat(tx.ToBytes());
            }

            return bytes.ToArray();
        }

        public Byte[] HeaderHash()
        {
            var version = BitConverter.GetBytes(Version);
            var timeStamp = BitConverter.GetBytes(TimeStamp);
            var bits = BitConverter.GetBytes(Bits);
            var nonce = BitConverter.GetBytes(Nonce);
            var bytes =  version
                         .Concat(PreviousBlockHash)
                         .Concat(MerkleRoot)
                         .Concat(timeStamp)
                         .Concat(bits)
                         .Concat(nonce)
                         .ToArray();

            SHA256 sha256 = SHA256Managed.Create();
            return sha256.ComputeHash(sha256.ComputeHash(bytes));
        }

        public Byte[] Hash()
        {
            var version = BitConverter.GetBytes(Version);
            var timeStamp = BitConverter.GetBytes(TimeStamp);
            var bits = BitConverter.GetBytes(Bits);
            var nonce = BitConverter.GetBytes(Nonce);
            var pcmBytes = PrimeChainMultiplier.ToByteArray();
            var pcmCount = new IntegerPayload((UInt64)pcmBytes.Length);
            var pcm = pcmCount.ToBytes().Concat(pcmBytes);
            var bytes = version
                         .Concat(PreviousBlockHash)
                         .Concat(MerkleRoot)
                         .Concat(timeStamp)
                         .Concat(bits)
                         .Concat(nonce)
                         .Concat(pcm)
                         .ToArray();

            SHA256 sha256 = SHA256Managed.Create();
            return sha256.ComputeHash(sha256.ComputeHash(bytes));
        }
    }

    public class TxInputPayload : Payload
    {
        public TxOutPointPayload PreviousTransaction { get; }
        public UnknownPayload Script { get; }
        public UInt32 Sequence { get; }

        public TxInputPayload(
            TxOutPointPayload previousTx,
            UnknownPayload script,
            UInt32 sequence
        )
        {
            PreviousTransaction = previousTx;
            Script = script;
            Sequence = sequence;
        }

        public TxInputPayload(byte[] bytes)
        {
            PreviousTransaction = new TxOutPointPayload(bytes);
            var remaining = bytes.Skip(PreviousTransaction.ToBytes().Length);

            var scriptLength = new IntegerPayload(remaining.ToArray());
            remaining = remaining.Skip(scriptLength.ToBytes().Length);

            Script = new UnknownPayload(remaining.Take((Int32)scriptLength.Integer).ToArray());
            remaining = remaining.Skip(Script.ToBytes().Length);

            Sequence = BitConverter.ToUInt32(remaining.ToArray(), 0);
        }

        public override byte[] ToBytes()
        {
            var txOutPointBytes = PreviousTransaction.ToBytes();
            var scriptBytes = Script.ToBytes();
            var scriptLengthBytes = new IntegerPayload((UInt64)scriptBytes.Length).ToBytes();
            var sequenceBytes = BitConverter.GetBytes(Sequence);

            return txOutPointBytes
                   .Concat(scriptLengthBytes)
                   .Concat(scriptBytes)
                   .Concat(sequenceBytes)
                   .ToArray();
        }
    }

    public class TxOutPointPayload : Payload
    {
        public Byte[] TransactionHash { get; }
        public UInt32 Index { get; }
        
        public TxOutPointPayload(Byte[] txHash, UInt32 index)
        {
            TransactionHash = txHash;
            Index = index;
        }

        public TxOutPointPayload(byte[] bytes)
        {
            TransactionHash = bytes.Take(32).ToArray();
            Index = BitConverter.ToUInt32(bytes, 32);
        }

        public override byte[] ToBytes()
        {
            return TransactionHash.Concat(BitConverter.GetBytes(Index)).ToArray();
        }
    }

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
