
namespace Caesura.LibNetwork
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Net;
    using System.Net.Http;
    using System.Net.Sockets;
    
    internal class InternalHttpServerHttpListener : IHttpServer
    {
        public const int DefaultIpAddress = 4988;
        private HttpListener Listener;
        
        // TODO: events for GET, DELETE, POST, PUT and PATCH here.
        // Also an event that triggers for all of them. And make
        // them async.
        // Also an event for unrecognized request names. Not an
        // outright error, but something to be informed of.
        
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
