
namespace Caesura.LibNetwork
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
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
        
        public bool Pending()
        {
            return listener.Pending();
        }
        
        public async Task<ITcpSession> AcceptTcpConnection(CancellationToken token)
        {
            while (!token.IsCancellationRequested && !Pending())
            {
                if (token.IsCancellationRequested)
                {
                    return TcpSession.Empty;
                }
                
                await Task.Delay(15, token);
            }
            
            var client  = listener.AcceptTcpClient();
            var session = new TcpSession(client, Config.ConnectionTimeoutTicks);
            return session;
        }
        
        public async Task<ITcpSession> Connect(string host, int port)
        {
            var client = new TcpClient();
            try
            {
                var session = new TcpSession(client, Config.ConnectionTimeoutTicks);
                await client.ConnectAsync(host, port);
                return session;
            }
            catch
            {
                client.Close();
                throw;
            }
        }
        
        public void Start()
        {
            listener.Start();
        }
        
        public void Stop()
        {
            listener.Stop();
        }
        
        public void Dispose()
        {
            Stop();
        }
    }
}
