using System;
using System.Net;
using System.Windows;

using PrimeNetwork;

namespace WinPrime
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ConnectionManager connections;

        public MainWindow()
        {
            InitializeComponent();
            connections = new ConnectionManager(IPAddress.Parse("127.0.0.1"), 9911);
            connections.NewConnection += new EventHandler<NewConnectionEventArgs>(HandleNewConnection);

            // Really need to figure out how to run things in other threads.
            // Blocks the UI from loading until done.
            connections.Start();
            
            var message = string.Format("Made {0} connections!", connections.OutboundConnections.Count);
            MessageBox.Show(message);
        }

        void HandleNewConnection(object sender, NewConnectionEventArgs a)
        {
            ConnectionListBox.Items.Add(a.Connect.To);
        }
    }
}
