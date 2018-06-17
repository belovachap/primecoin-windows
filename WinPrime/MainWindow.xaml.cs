using System;
using System.Net;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Controls;

using PrimeNetwork;

namespace WinPrime
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ConnectionManager Connections;

        public MainWindow()
        {
            InitializeComponent();

            Connections = new ConnectionManager(IPAddress.Parse("127.0.0.1"), 9911);
            Connections.NewConnection += new EventHandler<NewConnectionEventArgs>(HandleNewConnection);

            // Really need to figure out how to run things in other threads.
            // Blocks the UI from loading until done.
            Connections.Start();

            ConnectionListBox.SelectionChanged +=
                new SelectionChangedEventHandler(HandleConnectionSelectionChanged);
        }

        void HandleNewConnection(object sender, NewConnectionEventArgs a)
        {
            ConnectionListBox.Items.Add(a.Connect);
        }

        void HandleConnectionSelectionChanged(object sender, SelectionChangedEventArgs a)
        {
            var connection = (Connection)ConnectionListBox.SelectedItem;
            if (connection == null)
            {
                ConnectionFromTextBlock.Text = "";
                ConnectionToTextBlock.Text = "";
                ConnectionPortTextBlock.Text = "";
                ConnectionServicesTextBlock.Text = "";
                ConnectionProtocolVersionTextBlock.Text = "";
                ConnectionStartingHeightTextBlock.Text = "";
            }
            else
            {
                ConnectionFromTextBlock.Text = connection.From.ToString();
                ConnectionToTextBlock.Text = connection.To.ToString();
                ConnectionPortTextBlock.Text = connection.Port.ToString();
                ConnectionServicesTextBlock.Text = connection.Services.ToString();
                ConnectionProtocolVersionTextBlock.Text = connection.ProtocolVersion.ToString();
                ConnectionStartingHeightTextBlock.Text = connection.StartHeight.ToString();
            }
        }
    }
}
