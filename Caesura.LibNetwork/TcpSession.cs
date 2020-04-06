
namespace Caesura.LibNetwork
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Net.Sockets;
    
    internal class TcpSession : IDisposable
    {
        private int starter_ticks;
        public bool Active { get; private set; }
        public int TicksLeft { get; private set; }
        public TcpClient Client { get; private set; }
        
        public TcpSession(TcpClient client, int ticks)
        {
            starter_ticks = ticks;
            Active        = true;
            Client        = client;
        }
        
        public void TickDown()
        {
            TicksLeft = TicksLeft <= 0 ? 0 : TicksLeft - 1;
            
            if (TicksLeft <= 0)
            {
                Active = false;
                Close();
            }
        }
        
        public void ResetTicks()
        {
            TicksLeft = starter_ticks;
        }
        
        public void Close()
        {
            Active = false;
            Client.Close();
        }
        
        public void Dispose()
        {
            Close();
        }
    }
}
