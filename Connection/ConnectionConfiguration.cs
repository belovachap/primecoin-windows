using System;

namespace Connection
{
    public class ConnectionConfiguration
    {
        public UInt16 DefaultPort { get; }
        public String DNSSeed { get; }

        public ConnectionConfiguration(UInt16 defaultPort, String dnsSeed)
        {
            DefaultPort = defaultPort;
            DNSSeed = dnsSeed;
        }
    }
}
