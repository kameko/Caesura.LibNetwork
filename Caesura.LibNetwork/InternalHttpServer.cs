
namespace Caesura.LibNetwork
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Net;
    using System.Net.Http;
    using System.Net.Sockets;
    
    internal class InternalHttpServer : IDisposable
    {
        public const int DefaultIpAddress = 4988;
        private HttpListener Listener;
        
        // TODO: events for GET, DELETE, POST, PUT and PATCH here.
        // Also an event that triggers for all of them. And make
        // them async.
        // Also an event for unrecognized request names. Not an
        // outright error, but something to be informed of.
        
        public InternalHttpServer(string address)
        {
            Listener = new HttpListener();
            Listener.Prefixes.Add(address);
        }
        
        public InternalHttpServer(IEnumerable<string> addresses)
        {
            Listener = new HttpListener();
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
