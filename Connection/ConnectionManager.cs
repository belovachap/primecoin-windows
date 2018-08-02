using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

using Protocol;

namespace Connection
{

    public class NewConnectionEventArgs : EventArgs
    {
        public Connection Connection;

        public NewConnectionEventArgs(Connection connection)
        {
            Connection = connection;  
        }
    }

    public class ConnectionManager
    {
        ConnectionConfiguration ConnectionConfig;
        ProtocolConfiguration ProtocolConfig;
        List<IPAddress> DnsIPAddresses;
        public List<Connection> OutboundConnections;

        public event EventHandler<NewConnectionEventArgs> NewConnection;

        public ConnectionManager(
            ConnectionConfiguration connectionConfig,
            ProtocolConfiguration protocolConfig
        )
        {
            ConnectionConfig = connectionConfig;
            ProtocolConfig = protocolConfig;
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
                    client.Connect(toAddress, ConnectionConfig.DefaultPort);
                    var connection = new Connection(
                        from: IPAddress.Loopback,
                        to: toAddress,
                        port: ConnectionConfig.DefaultPort,
                        connectionConfig: ConnectionConfig,
                        protocolConfig: ProtocolConfig,
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
            var host = Dns.GetHostEntry(ConnectionConfig.DNSSeed);
            return new List<IPAddress>(host.AddressList);
        }
    }
}
