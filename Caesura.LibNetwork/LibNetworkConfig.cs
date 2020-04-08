
namespace Caesura.LibNetwork
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Net;
    
    public class LibNetworkConfig
    {
        public LibNetworkFactories Factories { get; set; }
        public IPAddress IP { get; set; }
        public int Port { get; set; }
        public int MaxConnections { get; set; }
        public int HeaderAmountLimit { get; set; }
        public int ConnectionTimeoutTicks { get; set; }
        public int DefaultTimeoutMilliseconds { get; set; }
        
        public LibNetworkConfig()
        {
            Factories                   = LibNetworkFactories.GetDefault();
            IP                          = IPAddress.Any;
            Port                        = 4988;
            MaxConnections              = 20;
            HeaderAmountLimit           = 100;
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
        public Func<LibNetworkConfig, StreamReader, CancellationToken, IHttpRequest> HttpRequestFactory { get; set; }
        public Func<LibNetworkConfig, ITcpSessionFactory> TcpSessionFactoryFactory { get; set; }
        
        public LibNetworkFactories()
        {
            HttpRequestFactory = (c, s, t) => HttpRequest.FromStream(s, c.HeaderAmountLimit, t);
            TcpSessionFactoryFactory = (c) => new TcpSessionFactory(c);
        }
        
        public static LibNetworkFactories GetDefault()
        {
            return new LibNetworkFactories();
        }
    }
}
