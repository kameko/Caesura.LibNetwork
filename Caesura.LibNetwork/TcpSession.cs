
namespace Caesura.LibNetwork
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.IO;
    using System.Net.Sockets;
    
    internal class TcpSession : ITcpSession
    {
        private int starter_ticks;
        private TcpClient _client;
        private StreamWriter _writer;
        public TcpSessionState State { get; private set; }
        public int TicksLeft { get; private set; }
        public Guid Id { get; private set; }
        public StreamReader Output { get; private set; }
        public bool DataAvailable => _client.GetStream().DataAvailable;
        
        public TcpSession(TcpClient client, int ticks)
        {
            _client       = client;
            starter_ticks = ticks;
            TicksLeft     = ticks;
            State         = TcpSessionState.Ready;
            Id            = Guid.NewGuid();
            _writer       = new StreamWriter(_client.GetStream());
            Output        = new StreamReader(_client.GetStream());
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
        
        public void TickDown()
        {
            if (starter_ticks <= -1)
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
            TicksLeft = starter_ticks;
        }
        
        public void Close()
        {
            State = TcpSessionState.Closed;
            _client.Close();
            Output.Close();
        }
        
        public void Dispose()
        {
            Close();
        }
    }
}
