using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol
{
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
}
