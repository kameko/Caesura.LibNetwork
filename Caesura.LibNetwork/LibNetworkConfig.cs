
namespace Caesura.LibNetwork
{
    using System;
    
    public class LibNetworkConfig
    {
        // HTTP server config
        public int MaxConnections { get; set; }
        public int HeaderAmountLimit { get; set; }
        public int HeaderCharReadLimit { get; set; }
        public int BodyCharReadLimit { get; set; }
        public int DefaultTimeoutMilliseconds { get; set; }
        
        public LibNetworkConfig()
        {
            MaxConnections              = 20;
            HeaderAmountLimit           = 100;
            HeaderCharReadLimit         = 1_048_576;
            BodyCharReadLimit           = int.MaxValue;
            DefaultTimeoutMilliseconds  = 5_000;
        }
        
        public static LibNetworkConfig GetDefault()
        {
            return new LibNetworkConfig();
        }
    }
}
