using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Numerics;

using Connection;

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

        public Boolean DesiredState; // true is "Running". false is "Stopped"
        Object StateLock = new Object();
        public Boolean State;

        NetworkConfiguration NetworkConfig;

        public Miner(NetworkConfiguration networkConfig)
        {
            NetworkConfig = networkConfig;
        }

        public void Start(BlockPayload block)
        {
            DesiredState = true;
            lock (StateLock)
            {
                State = true;

                while (DesiredState == true)
                {
                    Boolean AcceptableNonceFound = false;
                    for(block.Nonce = 0; block.Nonce < 0xFFFF0000; block.Nonce++)
                    {
                        var headerHashBytes = block.HeaderHash().AsEnumerable();
                        headerHashBytes = headerHashBytes.Concat(new Byte[] { 0x00 });
                        var headerHash = new BigInteger(headerHashBytes.ToArray());
                        if (headerHash < NetworkConfig.MinimumHeaderHash)
                        {
                            continue;
                        }
                        if (Algorithms.ProbablePrimalityTestWithTrialDivision(headerHash))
                        {
                            AcceptableNonceFound = true;
                            break;
                        }
                    }
                    if(!AcceptableNonceFound)
                    {
                        break;
                    }
                    


                    // When block is found, signal the event.
                    //var minedBlock = new BlockPayload();
                    //NewBlockMined?.Invoke(this, new NewBlockMinedEventArgs(minedBlock));
                }

                State = false;
            }
        }

        public void Stop()
        {
            DesiredState = false;
            // Wait for Start to release StartLock.
            lock (StateLock) { };
        }
    }
}
