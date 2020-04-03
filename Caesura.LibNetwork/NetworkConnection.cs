
namespace Caesura.LibNetwork
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Net.Http;
    
    public class NetworkConnection : IDisposable
    {
        internal HttpClient HttpNetwork { get; private set; }
        
        internal NetworkConnection()
        {
            HttpNetwork = null!;
        }
        
        private void Init(LibNetworkConfig config)
        {
            if (config.HttpHandler is null)
            {
                HttpNetwork = new HttpClient();
            }
            else
            {
                HttpNetwork = new HttpClient(config.HttpHandler, config.DisposeHttpHandler);
            }
        }
        
        public static NetworkConnection Create() => Create(LibNetworkConfig.GetDefault());
        
        public static NetworkConnection Create(LibNetworkConfig config)
        {
            var nc = new NetworkConnection();
            nc.Init(config);
            return nc;
        }
        
        public void Cancel()
        {
            HttpNetwork.CancelPendingRequests();
        }
        
        public void Dispose()
        {
            Cancel();
            HttpNetwork.Dispose();
        }
    }
}
