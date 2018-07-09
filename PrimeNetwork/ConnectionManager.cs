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
        NetworkConfiguration NetworkConfig;
        List<IPAddress> DnsIPAddresses;
        public List<Connection> OutboundConnections;

        public event EventHandler<NewConnectionEventArgs> NewConnection;

        public ConnectionManager(NetworkConfiguration networkConfig)
        {
            NetworkConfig = networkConfig;
            DnsIPAddresses = GetDnsIPAddresses();
            OutboundConnections = new List<Connection>();
        }

        public void Start()
        {
            Byte count = 0;
            foreach(IPAddress toAddress in DnsIPAddresses)
            {
                try
                {
                    var client = new TcpClient();
                    client.Connect(toAddress, NetworkConfig.DefaultPort);
                    var connection = new Connection(
                        from: IPAddress.Loopback,
                        to: toAddress,
                        port: NetworkConfig.DefaultPort,
                        networkConfig: NetworkConfig,
                        client: client
                    );
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
                catch
                {
                    continue;
                }
            }
        }

        public List<IPAddress> GetDnsIPAddresses()
        {
            var host = Dns.GetHostEntry(NetworkConfig.DNSSeed);
            return new List<IPAddress>(host.AddressList);
        }
    }
}
