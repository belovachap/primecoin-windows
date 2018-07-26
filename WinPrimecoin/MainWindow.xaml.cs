using System;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;

using Blockchain;
using Connection;
using Miner;
using Protocol;

namespace WinPrimecoin
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ConnectionManager Connections;
        BlockchainManager Blockchains;
        MinerManager Miners;

        public MainWindow(Boolean useTestnet)
        {
            InitializeComponent();

            ConnectionConfiguration connectionConfig;
            ProtocolConfiguration protocolConfig;
            if (useTestnet)
            {
                connectionConfig = new ConnectionConfiguration(
                    defaultPort: 9913,
                    dnsSeed: "tseed.primecoin.me"
                );
                protocolConfig = new ProtocolConfiguration(
                    magic: 0xC3CBFEFB,
                    minimumChainLength: 2,
                    maximumChainLength: 99,
                    minimumHeaderHash: new BigInteger(1) << 255,
                    minimumChainOrigin: new BigInteger(1) << 255,
                    maximumChainOrigin: new BigInteger(1) << 2000 - 1
                );
            }
            else
            {
                connectionConfig = new ConnectionConfiguration(
                    defaultPort: 9911,
                    dnsSeed: "seed.primecoin.me"
                );
                protocolConfig = new ProtocolConfiguration(
                    magic: 0xE7E5E7E4,
                    minimumChainLength: 6,
                    maximumChainLength: 99,
                    minimumHeaderHash: new BigInteger(1) << 255,
                    minimumChainOrigin: new BigInteger(1) << 255,
                    maximumChainOrigin: new BigInteger(1) << 2000 - 1
                );
            }

            ConnectionListBox.SelectionChanged +=
                new SelectionChangedEventHandler(HandleConnectionSelectionChanged);
            ConnectionMessagesOutListBox.SelectionChanged +=
                new SelectionChangedEventHandler(HandleMessagesOutSelectionChanged);
            ConnectionMessagesInListBox.SelectionChanged +=
                new SelectionChangedEventHandler(HandleMessagesInSelectionChanged);

            Miners = new MinerManager(protocolConfig);
            MinerListBox.Items.Add(Miners.CPUMiner);

            Blockchains = new BlockchainManager();
            Blockchains.NewBlockchain += new EventHandler<NewBlockchainEventArgs>(HandleNewBlockchain);
            Blockchains.NewBestBlock += new EventHandler<NewBestBlockEventArgs>(HandleNewBestBlock);
            Blockchains.NewBestBlock += new EventHandler<NewBestBlockEventArgs>(Miners.HandleNewBestBlock);
            
            Connections = new ConnectionManager(connectionConfig, protocolConfig);
            Connections.NewConnection += new EventHandler<NewConnectionEventArgs>(Blockchains.HandleNewConnection);
            Connections.NewConnection += new EventHandler<NewConnectionEventArgs>(Miners.HandleNewConnection);
            Connections.NewConnection += new EventHandler<NewConnectionEventArgs>(HandleNewConnection);

            Connections.Start();
        }

        void HandleNewConnection(object sender, NewConnectionEventArgs a)
        {
            ConnectionListBox.Items.Add(a.Connect);
        }

        void HandleNewBlockchain(object sender, NewBlockchainEventArgs a)
        {
            BlockchainListBox.Items.Add(a.Blockchain);
        }

        void HandleNewBestBlock(object sender, NewBestBlockEventArgs a)
        {
            Dispatcher.Invoke(() =>
            {
                BestBlockReceivedTextBlock.Text = DateTime.Now.ToString();
                BestBlockVersionTextBlock.Text = a.Block.Version.ToString();
                BestBlockTimeStampTextBlock.Text = a.Block.TimeStamp.ToString();
                BestBlockBitsTextBlock.Text = a.Block.Bits.ToString();
                BestBlockNonceTextBlock.Text = a.Block.Nonce.ToString();
                BestBlockPCMTextBlock.Text = a.Block.PrimeChainMultiplier.ToString();
                BestBlockTransactionsTextBlock.Text = a.Block.Transactions.Count.ToString();
                BestBlockMerkleRootTextBlock.Text = BitConverter.ToString(a.Block.MerkleRoot).Replace("-", string.Empty);
                BestBlockHeaderHashTextBlock.Text = BitConverter.ToString(a.Block.Hash()).Replace("-", string.Empty);
            });
        }

        void HandleConnectionSelectionChanged(object sender, SelectionChangedEventArgs a)
        {
            ConnectionFromTextBlock.Text = "";
            ConnectionToTextBlock.Text = "";
            ConnectionPortTextBlock.Text = "";
            ConnectionServicesTextBlock.Text = "";
            ConnectionProtocolVersionTextBlock.Text = "";
            ConnectionStartingHeightTextBlock.Text = "";
            ConnectionAliveTextBlock.Text = "";
            ConnectionMessagesInCountTextBlock.Text = "0";
            ConnectionMessagesOutCountTextBlock.Text = "0";
            ConnectionMessagesOutListBox.Items.Clear();
            ConnectionMessagesInListBox.Items.Clear();

            var connection = (Connection.Connection)ConnectionListBox.SelectedItem;
            if (connection != null)
            {
                ConnectionFromTextBlock.Text = connection.From.ToString();
                ConnectionToTextBlock.Text = connection.To.ToString();
                ConnectionPortTextBlock.Text = connection.Port.ToString();
                ConnectionServicesTextBlock.Text = connection.Services.ToString();
                ConnectionProtocolVersionTextBlock.Text = connection.ProtocolVersion.ToString();
                ConnectionStartingHeightTextBlock.Text = connection.StartHeight.ToString();
                ConnectionAliveTextBlock.Text = connection.Alive.ToString();
                foreach (MessagePayload message in connection.SentMessages)
                {
                    ConnectionMessagesOutListBox.Items.Add(message);
                }
                ConnectionMessagesOutCountTextBlock.Text = connection.SentMessages.Count.ToString();
                foreach (MessagePayload message in connection.ReceivedMessages)
                {
                    ConnectionMessagesInListBox.Items.Add(message);
                }
                ConnectionMessagesInCountTextBlock.Text = connection.ReceivedMessages.Count.ToString();
            }
        }

        void HandleMessagesOutSelectionChanged(object sender, SelectionChangedEventArgs a)
        {
            var message = (MessagePayload)ConnectionMessagesOutListBox.SelectedItem;
            if (message != null)
            {
                var messageString = String.Format(
                    "Command: {0}\nBytes: {1}",
                    message.Command,
                    message.CommandPayload
                );
                MessageBox.Show(messageString);
            }
        }

        void HandleMessagesInSelectionChanged(object sender, SelectionChangedEventArgs a)
        {
            var message = (MessagePayload)ConnectionMessagesInListBox.SelectedItem;
            if (message != null)
            {
                var messageString = String.Format(
                    "Command: {0}\nCommand Payload: {1}\nBytes: ",
                    message.Command,
                    message.CommandPayload
                );
                foreach(Byte b in message.CommandPayload.ToBytes())
                {
                    messageString += b.ToString("X2") + " ";
                }
                MessageBox.Show(messageString);
            }
        }
    }
}
