using System;
using System.Collections.Generic;
using PrimeNetwork;

namespace PrimeBlockchain
{

    public class NewBestBlockEventArgs : EventArgs
    {
        public BlockPayload Block;

        public NewBestBlockEventArgs(BlockPayload block)
        {
            Block = block;
        }
    }

    public class Blockchain
    {
        public event EventHandler<NewBestBlockEventArgs> NewBestBlock;

        Connection Connection;

        public Blockchain(Connection connection)
        {
            Connection = connection;
            Connection.NewMessage += new EventHandler<NewMessageEventArgs>(HandleNewMessage);
        }

        void HandleNewMessage(object sender, NewMessageEventArgs a)
        {
            switch(a.Message.Command)
            {
                case "block":
                    ProcessBlockPayload((BlockPayload)a.Message.CommandPayload);
                    break;

                case "inv":
                    ProcessInvPayload((InvPayload)a.Message.CommandPayload);
                    break;

                default:
                    break;
            }  
        }

        void ProcessBlockPayload(BlockPayload payload)
        {
            Algorithms.CheckProofOfWork(payload, Connection.NetworkConfig);
            // Signal as newest block b/c lazy for a second...
            NewBestBlock?.Invoke(this, new NewBestBlockEventArgs(payload));
        }

        void ProcessInvPayload(InvPayload payload)
        {
            var unknownBlocks = new List<InvEntryPayload>();
            foreach (InvEntryPayload entry in payload.Entries)
            {
                if (entry.Type == InvEntryType.MSG_BLOCK)
                {
                    unknownBlocks.Add(entry);
                }
            }
            Connection.SendGetDataMessage(new InvPayload(unknownBlocks));
        }
    }
}
