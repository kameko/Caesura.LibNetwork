
namespace Caesura.LibNetwork
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Net;
    using System.Net.Http;
    using System.Net.Sockets;
    
    // https://docs.microsoft.com/en-us/dotnet/api/system.net.httplistener?redirectedfrom=MSDN&view=netcore-3.0 
    
    internal class InternalHttpServerHttpListener : IHttpServer
    {
        public const int DefaultIpAddress = 4988;
        private HttpListener Listener;
        
        private InternalHttpServerHttpListener()
        {
            if (!HttpListener.IsSupported)
            {
                throw new PlatformNotSupportedException("HttpListener is not supported on this platform.");
            }
            
            Listener = new HttpListener();
        }
        
        public InternalHttpServerHttpListener(string address) : this()
        {
            Listener.Prefixes.Add(address);
        }
        
        public InternalHttpServerHttpListener(IEnumerable<string> addresses) : this()
        {
            foreach (var address in addresses)
            {
                Listener.Prefixes.Add(address);
            }
        }
        
        public Task StartAsync()
        {
            throw new NotImplementedException();
        }
        
        public void Start()
        {
            Listener.Start();
        }
        
        public void Stop()
        {
            Listener.Stop();
        }
        
        public void Dispose()
        {
            Stop();
        }
    }
}
