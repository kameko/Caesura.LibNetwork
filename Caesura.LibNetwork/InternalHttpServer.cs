
namespace Caesura.LibNetwork
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Net;
    using System.Net.Http;
    using System.Net.Sockets;
    
    internal class InternalHttpServer : IHttpServer
    {
        private TcpListener Listener;
        
        // TODO: events for GET, DELETE, POST, PUT and PATCH here.
        // Also an event that triggers for all of them. And make
        // them async.
        // Also an event for unrecognized request names. Not an
        // outright error, but something to be informed of.
        
        public InternalHttpServer(IPAddress ip, int port)
        {
            var nport = port <= 0 ? HttpServers.DefaultIpAddress : port;
            
            Listener = new TcpListener(ip, nport);
        }
        
        public InternalHttpServer(string ip, int port) : this(IPAddress.Parse(ip), port)
        {
            
        }
        
        public InternalHttpServer(int port) : this(IPAddress.IPv6Loopback, port)
        {
            
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
