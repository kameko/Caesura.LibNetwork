
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
        private TcpListener Listener;
        
        // TODO: events for GET, DELETE, POST and PUT here.
        // Also an event that triggers for all of them.
        
        public InternalHttpServer(IPAddress ip, int port)
        {
            port = port <= 0 ? DefaultIpAddress : port;
            
            Listener = new TcpListener(ip, port);
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
