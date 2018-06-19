using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace PrimeNetwork
{

    public class NewConnectionEventArgs : EventArgs
    {
        public Connection Connect;

        public NewConnectionEventArgs(Connection connection)
        {
            Connect = connection;  
        }
    }

    public class ConnectionManager
    {
        IPAddress MyAddress;
        UInt16 MyPort;
        List<IPAddress> DnsIPAddresses;
        public List<Connection> OutboundConnections;

        public event EventHandler<NewConnectionEventArgs> NewConnection;

        public ConnectionManager(IPAddress myAddress, UInt16 myPort)
        {
            MyAddress = myAddress;
            MyPort = myPort;
            DnsIPAddresses = GetDnsIPAddresses();
            OutboundConnections = new List<Connection>();
        }

        public void Start()
        {
            Byte count = 0;
            foreach(IPAddress toAddress in DnsIPAddresses)
            {
                var client = new TcpClient();
                try
                {
                    client.Connect(toAddress, 9911);
                }
                catch (SocketException)
                {
                    continue;
                }

                var connection = new Connection(MyAddress, toAddress, 9911, client);
                OutboundConnections.Add(connection);
                NewConnection(this, new NewConnectionEventArgs(connection));
                connection.StartReceivingMessages();

                // Just get eight connections for now.
                count++;
                if (count >= 8)
                {
                    break;
                }
            }
        }

        public List<IPAddress> GetDnsIPAddresses()
        {
            var host = Dns.GetHostEntry("seed.primecoin.me");
            return new List<IPAddress>(host.AddressList);
        }
    }
}
