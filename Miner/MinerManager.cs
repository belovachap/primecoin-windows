using System;
using System.Collections.Generic;

using Blockchain;
using Connection;
using Protocol;

namespace Miner
{
    public class NewMinerEventArgs : EventArgs
    {
        public Miner Miner;

        public NewMinerEventArgs(Miner miner)
        {
            Miner = miner;
        }
    }

    public class NewMiningAddressEventArgs : EventArgs
    {
        public String MiningAddress;

        public NewMiningAddressEventArgs(String miningAddress)
        {
            MiningAddress = miningAddress;
        }
    }
    public class MinerManager
    {
        public event EventHandler<NewMinerEventArgs> NewMiner;

        public Miner CPUMiner;

        object ConnectionsLock = new Object();
        List<Connection.Connection> Connections;

        object DataLock = new Object();
        BlockPayload BestBlock;
        String MiningAddress;
        
        public MinerManager(ProtocolConfiguration protocolConfig)
        {
            CPUMiner = new Miner(protocolConfig);
            NewMiner?.Invoke(this, new NewMinerEventArgs(CPUMiner));
        }

        public void HandleNewConnection(object sender, NewConnectionEventArgs a)
        {
            lock (ConnectionsLock)
            {
                // Remove all dead connections.
                // Add new connection.
            }
        }

        public void HandleNewBestBlock(object sender, NewBestBlockEventArgs a)
        {
            lock (DataLock)
            {
                // CPUMiner.Stop();
                BestBlock = a.Block;
                // Check if we have a good mining address.
                // Generate new block header to mine on.
                // CPUMiner.Start(newBlockHeader);
            }
        }

        public void HandleNewMiningAddress(object sender, NewMiningAddressEventArgs a)
        {
            lock (DataLock)
            {
                // CPUMiner.Stop();
                MiningAddress = a.MiningAddress;
                // Check if address is good.
                // Generate new block header to mine on.
                // CPUMiner.Start(newBlockHeader);
            }
        }

        public void HandleNewBlockMined(object sender, NewBlockMinedEventArgs a)
        {
            lock (ConnectionsLock)
            {
                // Attempt to broadcast block message to connections.
            }
        }
    }
}
