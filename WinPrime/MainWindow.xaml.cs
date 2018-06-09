using System.Windows;
using System.Net;

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

            // How do you subscribe to an event?
            // connections.NewConnection += HandleNewConnection;
        }

        void HandleNewConnection(object sender, NewConnectionEventArgs a)
        {
            MessageBox.Show("New connection made!");
        }
    }
}
