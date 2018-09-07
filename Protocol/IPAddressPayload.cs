using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Protocol
{
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
}
