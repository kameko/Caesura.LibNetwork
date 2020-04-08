
namespace Caesura.LibNetwork
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Net;
    using Http;
    
    public class LibNetworkConfig
    {
        public LibNetworkFactories Factories { get; set; }
        public HttpConfig Http { get; set; }
        public IPAddress IP { get; set; }
        public int Port { get; set; }
        public int MaxConnections { get; set; }
        public int ConnectionTimeoutTicks { get; set; }
        public int DefaultTimeoutMilliseconds { get; set; }
        
        public LibNetworkConfig()
        {
            Factories                   = LibNetworkFactories.GetDefault();
            Http                        = HttpConfig.GetDefault();
            IP                          = IPAddress.Any;
            Port                        = 4988;
            MaxConnections              = 20;
            ConnectionTimeoutTicks      = 10_000;
            DefaultTimeoutMilliseconds  = 5_000;
        }
        
        public static LibNetworkConfig GetDefault()
        {
            return new LibNetworkConfig();
        }
    }
    
    public class LibNetworkFactories
    {
        
        public Func<LibNetworkConfig, ITcpSessionFactory> TcpSessionFactoryFactory { get; set; }
        
        public LibNetworkFactories()
        {
            TcpSessionFactoryFactory = (c) => new TcpSessionFactory(c);
        }
        
        public static LibNetworkFactories GetDefault()
        {
            return new LibNetworkFactories();
        }
    }
}
