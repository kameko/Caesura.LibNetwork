
namespace Caesura.LibNetwork.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using System.IO;
    
    public class MockTcpSessionFactory : ITcpSessionFactory
    {
        private LibNetworkConfig Config;
        private bool running;
        private Dictionary<int, MemoryStream> streams;
        private MemoryStream? simulated_stream;
        private int simulated_port;
        public bool Running => running;
        
        public MockTcpSessionFactory(LibNetworkConfig config)
        {
            Config  = config;
            running = false;
            streams = new Dictionary<int, MemoryStream>();
        }
        
        public async Task SimulateConnection(MemoryStream stream, int port)
        {
            simulated_stream = stream;
            simulated_port   = port;
            
            var count = 30;
            while (simulated_port > 0)
            {
                count--;
                await Task.Delay(100);
                if (count <= 0)
                {
                    throw new InvalidOperationException("TEST: Gave up waiting for simulated connection.");
                }
            }
        }
        
        public bool Pending()
        {
            return !(simulated_stream is null);
        }
        
        public async Task<ITcpSession> AcceptTcpConnection(CancellationToken token)
        {
            while (!token.IsCancellationRequested && !Pending())
            {
                if (token.IsCancellationRequested)
                {
                    return TcpSessionFactory.Empty;
                }
                
                await Task.Delay(15);
            }
            
            var sm = simulated_stream!;
            var sp = simulated_port;
            simulated_stream = null;
            simulated_port = -1;
            
            var session = new MockTcpSession(sm, Config.TcpConnectionTimeoutTicks);
            streams.Add(sp, sm);
            
            return session;
        }
        
        public Task<ITcpSession> Connect(string host, int port) =>
            Task.Run<ITcpSession>(() => ConnectSync(host, port));
        
        private ITcpSession ConnectSync(string host, int port)
        {
            var remove_mes = new List<int>();
            MemoryStream? mstream = null;
            foreach (var (id, stream) in streams)
            {
                if (!stream.CanRead)
                {
                    remove_mes.Add(id);
                    continue;
                }
                if (id == port)
                {
                    mstream = stream;
                }
            }
            foreach (var remove in remove_mes)
            {
                streams.Remove(remove);
            }
            
            if (mstream is null)
            {
                mstream = new MemoryStream();
                streams.Add(port, mstream);
            }
            
            var session = new MockTcpSession(mstream, Config.TcpConnectionTimeoutTicks);
            return session;
        }
        
        public void Start()
        {
            running = true;
        }
        
        public void Stop()
        {
            running = false;
        }
        
        public void Dispose()
        {
            Stop();
        }
    }
}
