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
        List<Connection> OutboundConnections;

        public event EventHandler NewConnection;

        public ConnectionManager(IPAddress myAddress, UInt16 myPort)
        {
            // Lookup up addresses from "seed.primecoin.me"
            MyAddress = myAddress;
            MyPort = myPort;
            DnsIPAddresses = GetDnsIPAddresses();
            OutboundConnections = new List<Connection>();
        }

        public void Start()
        {
            foreach(IPAddress toAddress in DnsIPAddresses)
            {
                var client = new TcpClient(new IPEndPoint(toAddress, 9911));
                var connection = new Connection(MyAddress, toAddress, 9911, client);
                OutboundConnections.Add(connection);
                NewConnection(this, new NewConnectionEventArgs(connection));
            }
        }

        public List<IPAddress> GetDnsIPAddresses()
        {
            var addresses = new List<IPAddress>();
            return addresses;
        }
    }
}
