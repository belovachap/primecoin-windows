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
        public UInt32 MinimumChainLength { get; }
        public UInt32 MaximumChainLength { get; }
        public BigInteger MinimumHeaderHash { get; }
        public BigInteger MinimumChainOrigin { get; }
        public BigInteger MaximumChainOrigin { get; }

        public ProtocolConfiguration(
            UInt32 magic,
            UInt32 minimumChainLength,
            UInt32 maximumChainLength,
            BigInteger minimumHeaderHash,
            BigInteger minimumChainOrigin,
            BigInteger maximumChainOrigin
        )
        {
            Magic = magic;
            MinimumChainLength = minimumChainLength;
            MaximumChainLength = maximumChainLength;
            MinimumHeaderHash = minimumHeaderHash;
            MinimumChainOrigin = minimumChainLength;
            MaximumChainOrigin = maximumChainOrigin;
        }
    }
}
