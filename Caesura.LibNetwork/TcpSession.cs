
namespace Caesura.LibNetwork
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.IO;
    using System.Text;
    using System.Net.Sockets;
    
    internal class TcpSession : ITcpSession
    {
        private int _starter_ticks;
        private TcpClient _client;
        private StreamWriter _writer;
        public TcpSessionState State { get; private set; }
        public int TicksLeft { get; private set; }
        public Guid Id { get; private set; }
        public StreamReader Output { get; private set; }
        public bool DataAvailable => _client.GetStream().DataAvailable && State != TcpSessionState.Closed;
        
        public TcpSession(TcpClient client, int ticks)
        {
            _starter_ticks = ticks;
            _client        = client;
            _writer        = new StreamWriter(_client.GetStream(), Encoding.UTF8);
            TicksLeft      = ticks;
            State          = TcpSessionState.Ready;
            Id             = Guid.NewGuid();
            Output         = new StreamReader(_client.GetStream(), Encoding.UTF8);
        }
        
        internal TcpSession()
        {
            _starter_ticks = 0;
            _client        = null!;
            _writer        = null!;
            TicksLeft      = 0;
            State          = TcpSessionState.Closed;
            Id             = Guid.NewGuid();
            Output         = null!;
        }
        
        public async Task Write(string text, CancellationToken token)
        {
            if (State == TcpSessionState.Closed)
            {
                throw new TcpSessionNotActiveException("Session is no longer active.");
            }
            await _writer.WriteAsync(text);
            await _writer.FlushAsync();
        }
        
        public void Pulse()
        {
            CalculateTicks();
        }
        
        public void ResetTicks()
        {
            TicksLeft = _starter_ticks;
        }
        
        public void Close()
        {
            State = TcpSessionState.Closed;
            _client?.Close();
            Output?.Close();
        }
        
        private void CalculateTicks()
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
        
        public void Dispose()
        {
            Close();
        }
    }
}
