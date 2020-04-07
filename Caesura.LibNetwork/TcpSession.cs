
namespace Caesura.LibNetwork
{
    using System;
    using System.IO;
    using System.Net.Sockets;
    
    internal class TcpSession : ITcpSession
    {
        private int starter_ticks;
        private TcpClient _client;
        public TcpSessionState State { get; private set; }
        public int TicksLeft { get; private set; }
        public Guid Id { get; private set; }
        public StreamReader Reader { get; private set; }
        public StreamWriter Writer { get; private set; }
        public bool DataAvailable => _client.GetStream().DataAvailable;
        
        public TcpSession(TcpClient client, int ticks)
        {
            _client       = client;
            starter_ticks = ticks;
            TicksLeft     = ticks;
            State         = TcpSessionState.Ready;
            Id            = Guid.NewGuid();
            Reader        = new StreamReader(_client.GetStream());
            Writer        = new StreamWriter(_client.GetStream());
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
        }
        
        public void Dispose()
        {
            Close();
        }
    }
}
