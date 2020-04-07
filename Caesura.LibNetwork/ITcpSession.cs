
namespace Caesura.LibNetwork
{
    using System;
    using System.IO;
    
    public enum TcpSessionState
    {
        None   = 0,
        Ready  = 1,
        Closed = 2,
    }
    
    public interface ITcpSession : IDisposable
    {
        Guid Id { get; }
        int TicksLeft { get; }
        TcpSessionState State { get; }
        StreamReader Reader { get; }
        StreamWriter Writer { get; }
        bool DataAvailable { get; }
        
        void TickDown();
        void ResetTicks();
        void Close();
    }
}
