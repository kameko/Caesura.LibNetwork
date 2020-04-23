
namespace Caesura.LibNetwork.Tests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.IO;
    using System.Text;
    
    public class MockTcpSession : ITcpSession
    {
        private MemoryStream _memstream;
        private StreamWriter _writer;
        private bool data_available;
        
        public Guid Id { get; private set; }
        public TcpSessionState State { get; private set; }
        public StreamReader Output { get; private set; }
        public bool DataAvailable
        {
            get
            {
                var da = data_available;
                data_available = false;
                return da;
            }
        }
        
        public MockTcpSession(MemoryStream memstream, int ticks)
        {
            _memstream     = memstream;
            _writer        = new StreamWriter(memstream, Encoding.UTF8);
            data_available = false;
            
            Id             = Guid.NewGuid();
            State          = TcpSessionState.Ready;
            Output         = new StreamReader(memstream, Encoding.UTF8);
        }
        
        public async Task Write(string text, CancellationToken token)
        {
            if (State == TcpSessionState.Closed)
            {
                throw new TcpSessionNotActiveException("Session is no longer active.");
            }
            
            _memstream.Position = 0;
            await _writer.WriteAsync(text);
            await _writer.FlushAsync();
            _memstream.Position = 0;
            data_available = true;
        }
        
        public void Close()
        {
            State = TcpSessionState.Closed;
            _memstream.Close();
            Output.Close();
        }
        
        public void Dispose()
        {
            Close();
        }
    }
}
