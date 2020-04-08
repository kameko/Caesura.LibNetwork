
namespace Caesura.LibNetwork
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Net;
    using System.Net.Sockets;
    
    public class TcpSessionFactory : ITcpSessionFactory
    {
        private LibNetworkConfig Config;
        private TcpListener listener;
        
        public TcpSessionFactory(LibNetworkConfig config)
        {
            Config   = config;
            listener = new TcpListener(config.IP, config.Port);
        }
        
        public ITcpSession AcceptTcpConnection()
        {
            var client = listener.AcceptTcpClient();;
            var session = new TcpSession(client, Config.ConnectionTimeoutTicks);
            return session;
        }
        
        public void Start()
        {
            listener.Start();
        }
        
        public void Stop()
        {
            listener.Stop();
        }
    }
}
