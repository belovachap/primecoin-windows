using Base58Check;

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using System.Threading.Tasks;

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
        public event EventHandler<NewBlockMinedEventArgs> NewBlockMined;

        public Miner CPUMiner;

        object ConnectionsLock = new Object();
        List<Connection.Connection> Connections;

        object DataLock = new Object();
        BlockPayload BestBlock;
        Int64 SecondsSinceLastBlock;
        public Byte[] MineToPublicKeyHash;
        ProtocolConfiguration ProtocolConfig;

        public MinerManager(ProtocolConfiguration protocolConfig)
        {
            ProtocolConfig = protocolConfig;
            Connections = new List<Connection.Connection>();
            CPUMiner = new Miner(protocolConfig);
            CPUMiner.NewBlockMined += new EventHandler<NewBlockMinedEventArgs>(HandleNewBlockMined);
            NewMiner?.Invoke(this, new NewMinerEventArgs(CPUMiner));
        }

        public void HandleNewConnection(object sender, NewConnectionEventArgs a)
        {
            lock (ConnectionsLock)
            {
                // Remove all dead connections.
                Connections = Connections.Where(c => c.Alive).ToList();

                // Add new connection.
                Connections.Add(a.Connection);
            }
        }

        public void HandleNewBestBlock(object sender, NewBestBlockEventArgs a)
        {
            return;
            lock (DataLock)
            {
                BestBlock = a.Block;
                CPUMiner.Stop();
                // Check if we have a good mining address.
                // Generate new block to mine on.
                var difficulty = new DifficultyPayload(BestBlock.Bits);
                difficulty.Length++;
                difficulty.Fraction = 0;

                // TODO fill in the Coinbase transaction

                var miningBlock = new BlockPayload(
                    version: 2,
                    previousBlockHash: BestBlock.Hash(),
                    timeStamp: (UInt32)DateTime.UtcNow.Ticks,
                    bits: difficulty.ToBits(),
                    nonce: 0,
                    primeChainMultiplier: new BigInteger(1),
                    txs: new List<TransactionPayload>()
                );
                Task.Run(() => CPUMiner.Start(miningBlock));
            }
        }

        public void HandleNewMiningAddress(String miningAddress)
        {
            lock (DataLock)
            {
                CPUMiner.Stop();

                // Extract Public Key Hash from Primecoin Address.
                Byte[] addressBytes;
                try
                {
                    addressBytes = Base58CheckEncoding.Decode(miningAddress);
                }
                catch
                {
                    MineToPublicKeyHash = null;
                    return;
                }
                
                var idByte = addressBytes.Take(1).Single<Byte>();
                if (idByte != ProtocolConfig.PublicKeyID)
                {
                    MineToPublicKeyHash = null;
                    return;
                }
                MineToPublicKeyHash = addressBytes.Skip(1).Take(20).ToArray();

                // Generate new Coinbase transaction
                // beep boop, we can probably use format substituion
                // once we know what this scrip looks like in bytes.

                // Generate new block header to mine on.
                //var miningBlock = new BlockPayload(
                //    version: 2,
                //    previousBlockHash: BestBlock.Hash(),
                //    timeStamp: (UInt32)DateTime.UtcNow.Ticks,
                //    bits: difficulty.ToBits(),
                //    nonce: 0,
                //    primeChainMultiplier: new BigInteger(1),
                //    txs: new List<TransactionPayload>()
                //);
                //CPUMiner.Start(miningBlock);
            }
        }

        public void HandleNewBlockMined(object sender, NewBlockMinedEventArgs a)
        {
            lock (ConnectionsLock)
            {
                // Remove all dead connections.
                Connections = Connections.Where(c => c.Alive).ToList();

                // Broadcast new block!
                foreach (var c in Connections)
                {
                    c.SendBlockMessage(a.Block);
                }
            }

            // Notify watchers that a new block was mined and broadcast.
            NewBlockMined?.Invoke(this, a);
        }
    }
}
