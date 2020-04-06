
namespace Caesura.LibNetwork
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Net.Http;
    using System.Text.Json;
    
    public class LibNetworkConfig
    {
        // HTTP client config
        public HttpMessageHandler? HttpHandler { get; set; }
        public HttpCompletionOption CompletionOption { get; set; }
        public bool DisposeHttpHandler { get; set; }
        public JsonSerializerOptions JsonOptions { get; set; }
        // HTTP server config
        public int MaxConnections { get; set; }
        public int HeaderAmountLimit { get; set; }
        public int HeaderCharReadLimit { get; set; }
        public int BodyCharReadLimit { get; set; }
        
        public LibNetworkConfig()
        {
            HttpHandler         = null;
            CompletionOption    = HttpCompletionOption.ResponseContentRead;
            DisposeHttpHandler  = true;
            JsonOptions         = new JsonSerializerOptions()
            {
                WriteIndented   = true,
            };
            
            MaxConnections      = 20;
            HeaderAmountLimit   = 100;
            HeaderCharReadLimit = 1_048_576;
            BodyCharReadLimit   = int.MaxValue;
        }
        
        public static LibNetworkConfig GetDefault()
        {
            return new LibNetworkConfig();
        }
    }
}
