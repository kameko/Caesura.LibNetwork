
namespace Caesura.LibNetwork
{
    using System;
    using System.Net;
    
    public class LibNetworkConfig
    {
        public IPAddress IP { get; set; }
        public int Port { get; set; }
        public int MaxConnections { get; set; }
        public int HeaderAmountLimit { get; set; }
        public int HeaderCharReadLimit { get; set; }
        public int BodyCharReadLimit { get; set; }
        public int ConnectionTimeoutTicks { get; set; }
        public int DefaultTimeoutMilliseconds { get; set; }
        
        public LibNetworkConfig()
        {
            IP                          = IPAddress.Any;
            Port                        = 4988;
            MaxConnections              = 20;
            HeaderAmountLimit           = 100;
            HeaderCharReadLimit         = 1_048_576;
            BodyCharReadLimit           = int.MaxValue;
            ConnectionTimeoutTicks      = 10_000;
            DefaultTimeoutMilliseconds  = 5_000;
        }
        
        public static LibNetworkConfig GetDefault()
        {
            return new LibNetworkConfig();
        }
    }
}
