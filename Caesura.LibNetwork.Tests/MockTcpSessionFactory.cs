
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
        
        public void SimulateConnection(MemoryStream stream, int port)
        {
            simulated_stream = stream;
            simulated_port   = port;
        }
        
        public bool Pending()
        {
            throw new NotImplementedException();
        }
        
        public Task<ITcpSession> AcceptTcpConnection(CancellationToken token)
        {
            throw new NotImplementedException();
            /*
            while (simulated_stream is null)
            {
                await Task.Delay(15, token);
            }
            var stream = simulated_stream;
            simulated_stream = null;
            
            while (!token.IsCancellationRequested && !Pending())
            {
                if (token.IsCancellationRequested)
                {
                    // TODO: return a closed session.
                }
                
                await Task.Delay(15, token);
            }
            
            var session = new MockTcpSession(stream, Config.ConnectionTimeoutTicks);
            streams.Add(simulated_port, stream);
            return session;
            */
        }
        
        public Task<ITcpSession> Connect(string host, int port) =>
            Task.Run<ITcpSession>(() => ConnectSync(host, port));
        
        private ITcpSession ConnectSync(string host, int port)
        {
            var remove_mes = new List<int>();
            MemoryStream? stream = null;
            foreach (var stream_kvp in streams)
            {
                if (!stream_kvp.Value.CanRead)
                {
                    remove_mes.Add(stream_kvp.Key);
                    continue;
                }
                if (stream_kvp.Key == port)
                {
                    stream = stream_kvp.Value;
                }
            }
            foreach (var remove in remove_mes)
            {
                streams.Remove(remove);
            }
            
            if (stream is null)
            {
                stream = new MemoryStream();
                streams.Add(port, stream);
            }
            
            var session = new MockTcpSession(stream, Config.ConnectionTimeoutTicks);
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
