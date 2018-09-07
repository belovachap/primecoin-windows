using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace Protocol
{
    public class ProtocolConfiguration
    {
        public UInt32 Magic { get; }
        public Byte PublicKeyID { get; }
        public Byte ScriptID { get; }
        public UInt32 MinimumChainLength { get; }
        public UInt32 MaximumChainLength { get; }
        public BigInteger MinimumHeaderHash { get; }
        public BigInteger MinimumChainOrigin { get; }
        public BigInteger MaximumChainOrigin { get; }

        public ProtocolConfiguration(
            UInt32 magic,
            Byte publicKeyID,
            Byte scriptID,
            UInt32 minimumChainLength,
            UInt32 maximumChainLength,
            BigInteger minimumHeaderHash,
            BigInteger minimumChainOrigin,
            BigInteger maximumChainOrigin
        )
        {
            Magic = magic;
            PublicKeyID = publicKeyID;
            ScriptID = scriptID;
            MinimumChainLength = minimumChainLength;
            MaximumChainLength = maximumChainLength;
            MinimumHeaderHash = minimumHeaderHash;
            MinimumChainOrigin = minimumChainLength;
            MaximumChainOrigin = maximumChainOrigin;
        }

        public static ProtocolConfiguration MainnetConfig()
        {
            return new ProtocolConfiguration(
                    magic: 0xE7E5E7E4,
                    publicKeyID: 23,
                    scriptID: 83,
                    minimumChainLength: 6,
                    maximumChainLength: 99,
                    minimumHeaderHash: new BigInteger(1) << 255,
                    minimumChainOrigin: new BigInteger(1) << 255,
                    maximumChainOrigin: new BigInteger(1) << 2000 - 1
            );
        }

        public static ProtocolConfiguration TestnetConfig()
        {
            return new ProtocolConfiguration(
                    magic: 0xC3CBFEFB,
                    publicKeyID: 111,
                    scriptID: 196,
                    minimumChainLength: 2,
                    maximumChainLength: 99,
                    minimumHeaderHash: new BigInteger(1) << 255,
                    minimumChainOrigin: new BigInteger(1) << 255,
                    maximumChainOrigin: new BigInteger(1) << 2000 - 1
            );
        }
    }
}
