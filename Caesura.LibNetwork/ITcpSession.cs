
namespace Caesura.LibNetwork
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.IO;
    
    // TODO: Consider making a higher-level INetworkSession interface
    // and replace all ITcpSession instances with it. That way a user can
    // instead use a different protocol if they need to.
    
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
        StreamReader Output { get; }
        bool DataAvailable { get; }
        
        Task Write(string text, CancellationToken token);
        void Pulse();
        void ResetTicks();
        void Close();
    }
}
