using System;
using System.Linq;
using System.Text;
using System.Net;
using System.Security.Cryptography;

namespace PrimeNetwork
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
            Command = Command.TrimEnd(new char[]{'\0'});
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
                case "verack":
                    CommandPayload = new VerAckPayload(payload);
                    break;

                case "version":
                    CommandPayload = new VersionPayload(payload);
                    break;

                default:
                    throw new ArgumentException("unkown command!");
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
}
