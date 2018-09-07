using System;
using System.Linq;
using System.Numerics;

using Connection;
using Protocol;

namespace Miner
{
    public class NewBlockMinedEventArgs : EventArgs
    {
        public BlockPayload Block;

        public NewBlockMinedEventArgs(BlockPayload block)
        {
            Block = block;
        }
    }

    public class Miner
    {
        public event EventHandler<NewBlockMinedEventArgs> NewBlockMined;

        Boolean ShouldRun; // true is "Running". false is "Stopped"
        Object RunningLock = new Object();
        ProtocolConfiguration ProtocolConfig;

        public Miner(ProtocolConfiguration protocolConfig)
        {
            ProtocolConfig = protocolConfig;
        }

        public void Start(BlockPayload block)
        {
            ShouldRun = true;
            lock (RunningLock)
            {
                block.PrimeChainMultiplier = new BigInteger(1);
                while (ShouldRun)
                {
                    block.TimeStamp = (UInt32)DateTime.UtcNow.Ticks;
                    for (block.Nonce = 0; block.Nonce <= 0xFFFFFFFF; block.Nonce++)
                    {
                        if (!ShouldRun)
                        {
                            break;
                        }

                        var headerHash = block.HeaderHash();
                        if (headerHash < ProtocolConfig.MinimumHeaderHash)
                        {
                            continue;
                        }

                        try
                        {
                            Algorithm.CheckProofOfWork(block, ProtocolConfig);
                        }
                        catch
                        {
                            continue;
                        }

                        NewBlockMined?.Invoke(this, new NewBlockMinedEventArgs(block));
                    }
                }
            }
        }

        public void Stop()
        {
            ShouldRun = false;
            // Wait for Start to release RunningLock.
            lock (RunningLock) { };
        }
    }
}
