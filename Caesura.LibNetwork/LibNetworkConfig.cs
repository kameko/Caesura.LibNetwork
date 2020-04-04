
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
        public HttpMessageHandler? HttpHandler { get; set; }
        public HttpCompletionOption CompletionOption { get; set; }
        public bool DisposeHttpHandler { get; set; }
        public JsonSerializerOptions JsonOptions { get; set; }
        
        public LibNetworkConfig()
        {
            HttpHandler        = null;
            CompletionOption   = HttpCompletionOption.ResponseContentRead;
            DisposeHttpHandler = true;
            JsonOptions        = new JsonSerializerOptions()
            {
                WriteIndented = true,
            };
        }
        
        public static LibNetworkConfig GetDefault()
        {
            return new LibNetworkConfig();
        }
    }
}
