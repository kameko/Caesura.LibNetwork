
namespace Caesura.LibNetwork
{
    using System;
    using System.IO;
    using System.Net.Sockets;
    
    public enum TcpSessionState
    {
        None   = 0,
        Ready  = 1,
        Closed = 2,
    }
    
    internal class TcpSession : IDisposable
    {
        private int starter_ticks;
        public TcpSessionState State { get; private set; }
        public int TicksLeft { get; private set; }
        public Guid Id { get; private set; }
        public TcpClient Client { get; private set; }
        public StreamReader Reader { get; private set; }
        public StreamWriter Writer { get; private set; }
        
        public TcpSession(TcpClient client, int ticks)
        {
            starter_ticks = ticks;
            TicksLeft     = ticks;
            State         = TcpSessionState.Ready;
            Id            = Guid.NewGuid();
            Client        = client;
            Reader        = new StreamReader(Client.GetStream());
            Writer        = new StreamWriter(Client.GetStream());
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
            Client.Close();
        }
        
        public void Dispose()
        {
            Close();
        }
    }
}
