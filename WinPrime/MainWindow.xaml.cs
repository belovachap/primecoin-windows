using System;
using System.Net;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Controls;

using PrimeBlockchain;
using PrimeNetwork;


namespace WinPrime
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ConnectionManager Connections;
        BlockchainManager Blockchains;

        public MainWindow()
        {
            InitializeComponent();

            ConnectionListBox.SelectionChanged +=
                new SelectionChangedEventHandler(HandleConnectionSelectionChanged);
            ConnectionMessagesOutListBox.SelectionChanged +=
                new SelectionChangedEventHandler(HandleMessagesOutSelectionChanged);
            ConnectionMessagesInListBox.SelectionChanged +=
                new SelectionChangedEventHandler(HandleMessagesInSelectionChanged);

            Blockchains = new BlockchainManager();
            Blockchains.NewBlockchain += new EventHandler<NewBlockchainEventArgs>(HandleNewBlockchain);
            Blockchains.NewBestBlock += new EventHandler<NewBestBlockEventArgs>(HandleNewBestBlock);

            Connections = new ConnectionManager(IPAddress.Parse("127.0.0.1"), 9911);
            Connections.NewConnection += new EventHandler<NewConnectionEventArgs>(Blockchains.HandleNewConnection);
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
                BestBlockTextBlock.Text = DateTime.Now.ToString()
            );
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

            var connection = (Connection)ConnectionListBox.SelectedItem;
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
                    "Command: {0}\nBytes: {1}",
                    message.Command,
                    message.CommandPayload
                );
                MessageBox.Show(messageString);
            }
        }
    }
}
