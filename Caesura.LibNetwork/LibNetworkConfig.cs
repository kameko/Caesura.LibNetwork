
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
        public int ReadByteBufferSize { get; set; }
        
        public LibNetworkConfig()
        {
            HttpHandler        = null;
            CompletionOption   = HttpCompletionOption.ResponseContentRead;
            DisposeHttpHandler = true;
            JsonOptions        = new JsonSerializerOptions()
            {
                WriteIndented  = true,
            };
            
            MaxConnections     = 20;
            ReadByteBufferSize = 1024;
        }
        
        public static LibNetworkConfig GetDefault()
        {
            return new LibNetworkConfig();
        }
    }
}
