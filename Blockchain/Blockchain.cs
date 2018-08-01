using System;
using System.Collections.Generic;

using Connection;
using Protocol;

namespace Blockchain
{

    public class NewBestBlockEventArgs : EventArgs
    {
        public BlockPayload Block;
        public Int64 SecondsSinceLastBlock;

        public NewBestBlockEventArgs(BlockPayload block, Int64 secondsSinceLastBlock)
        {
            Block = block;
            SecondsSinceLastBlock = secondsSinceLastBlock;
        }
    }

    public class Blockchain
    {
        public event EventHandler<NewBestBlockEventArgs> NewBestBlock;

        Connection.Connection Connection;

        public Blockchain(Connection.Connection connection)
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
            Algorithm.CheckProofOfWork(payload, Connection.ProtocolConfig);
            // Signal as newest block b/c lazy for a second...
            // Also need to handle getting seconds since previous block...
            // Hardcoding it to 60 for now even though it will cause all the
            // mining difficulty to be off :)
            NewBestBlock?.Invoke(this, new NewBestBlockEventArgs(payload, 60));
        }

        void ProcessInvPayload(InvPayload payload)
        {
            var unknownBlocks = new List<InvEntryPayload>();
            foreach (InvEntryPayload entry in payload.Entries)
            {
                if (entry.Type == InvEntryPayload.InvEntryType.MSG_BLOCK)
                {
                    unknownBlocks.Add(entry);
                }
            }
            Connection.SendGetDataMessage(new InvPayload(unknownBlocks));
        }
    }
}
