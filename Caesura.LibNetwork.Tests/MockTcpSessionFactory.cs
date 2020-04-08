
namespace Caesura.LibNetwork.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.IO;
    using Http;
    
    public class MockTcpSessionFactory : ITcpSessionFactory
    {
        private LibNetworkConfig Config;
        private bool running;
        private Dictionary<int, MemoryStream> streams;
        
        public MockTcpSessionFactory(LibNetworkConfig config)
        {
            Config  = config;
            running = false;
            streams = new Dictionary<int, MemoryStream>();
        }
        
        public ITcpSession AcceptTcpConnection()
        {
            var stream  = new MemoryStream();
            var session = new MockTcpSession(stream, Config.ConnectionTimeoutTicks);
            return session;
        }
        
        public Task<ITcpSession> Connect(string host, int port)
        {
            return Task.Run<ITcpSession>(() =>
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
            });
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
