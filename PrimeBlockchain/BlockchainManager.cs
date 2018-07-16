using System;
using System.Collections.Generic;
using Connection;

namespace Blockchain
{
    public class NewBlockchainEventArgs : EventArgs
    {
        public Blockchain Blockchain;

        public NewBlockchainEventArgs(Blockchain blockchain)
        {
            Blockchain = blockchain;
        }
    }

    public class BlockchainManager
    {
        public event EventHandler<NewBestBlockEventArgs> NewBestBlock;
        public event EventHandler<NewBlockchainEventArgs> NewBlockchain;

        Object DataLock = new Object();
        List<Blockchain> Blockchains;
        List<BlockPayload> BlockchainsBestBlock;
        BlockPayload BestBlock;

        public BlockchainManager()
        {
            Blockchains = new List<Blockchain>();
            BlockchainsBestBlock = new List<BlockPayload>();
        }

        public void HandleNewConnection(object sender, NewConnectionEventArgs a)
        {
            lock(DataLock)
            {
                var blockchain = new Blockchain(a.Connect);
                Blockchains.Add(blockchain);
                BlockchainsBestBlock.Add(null);
                blockchain.NewBestBlock += new EventHandler<NewBestBlockEventArgs>(HandleNewBestBlock);
                NewBlockchain?.Invoke(this, new NewBlockchainEventArgs(blockchain));
            }
        }

        void HandleNewBestBlock(object sender, NewBestBlockEventArgs a)
        {
            lock (DataLock)
            {
                var index = Blockchains.IndexOf((Blockchain)sender);
                BlockchainsBestBlock[index] = a.Block;
                // Signal as newest block b/c lazy for a second...
                BestBlock = a.Block;
                NewBestBlock?.Invoke(this, new NewBestBlockEventArgs(BestBlock));
            }
        }
    }
}

