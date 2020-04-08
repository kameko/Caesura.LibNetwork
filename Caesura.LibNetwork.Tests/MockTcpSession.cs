
namespace Caesura.LibNetwork.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.IO;
    
    public class MockTcpSession : ITcpSession
    {
        private int _starter_ticks;
        private MemoryStream _memstream;
        private StreamWriter _writer;
        public Guid Id { get; private set; }
        public int TicksLeft { get; private set; }
        public TcpSessionState State { get; private set; }
        public StreamReader Output { get; private set; }
        public bool DataAvailable => _memstream.Length > 0;
        
        public MockTcpSession(MemoryStream memstream, int ticks)
        {
            _memstream     = memstream;
            _starter_ticks = ticks;
            _writer        = new StreamWriter(memstream);
            Id             = Guid.NewGuid();
            TicksLeft      = ticks;
            State          = TcpSessionState.Ready;
            Output         = new StreamReader(memstream);
        }
        
        public async Task Write(string text, CancellationToken token)
        {
            if (State == TcpSessionState.Closed)
            {
                throw new TcpSessionNotActiveException("Session is no longer active.");
            }
            await _writer.WriteAsync(text);
            await _writer.FlushAsync();
            _memstream.Position = 0;
        }
        
        public void TickDown()
        {
            if (_starter_ticks <= -1)
            {
                return;
            }
            
            TicksLeft = TicksLeft <= 0 ? 0 : TicksLeft - 1;
            
            if (TicksLeft <= 0)
            {
                Close();
            }
        }
        
        public void ResetTicks()
        {
            TicksLeft = _starter_ticks;
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