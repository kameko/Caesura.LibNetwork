
namespace Caesura.LibNetwork.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.IO;
    
    public class MockTcpSessionFactory : ITcpSessionFactory
    {
        private LibNetworkConfig Config;
        private bool running;
        private IEnumerator<int> portgen;
        private Dictionary<int, MemoryStream> streams;
        
        public MockTcpSessionFactory(LibNetworkConfig config, Func<IEnumerable<int>> port_generator)
        {
            Config  = config;
            running = false;
            portgen = port_generator().GetEnumerator();
            streams = new Dictionary<int, MemoryStream>();
        }
        
        public ITcpSession AcceptTcpConnection()
        {
            portgen.MoveNext();
            
            var stream  = new MemoryStream();
            var port    = portgen.Current;
            var session = new MockTcpSession(stream, Config.ConnectionTimeoutTicks);
            streams.Add(port, stream);
            return session;
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
