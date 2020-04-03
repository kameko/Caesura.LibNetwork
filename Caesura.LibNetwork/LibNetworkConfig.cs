
namespace Caesura.LibNetwork
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Net.Http;
    
    public class LibNetworkConfig
    {
        public HttpMessageHandler? HttpHandler { get; set; }
        public bool DisposeHttpHandler { get; set; }
        
        public LibNetworkConfig()
        {
            HttpHandler        = null;
            DisposeHttpHandler = true;
        }
        
        public static LibNetworkConfig GetDefault()
        {
            return new LibNetworkConfig();
        }
    }
}
