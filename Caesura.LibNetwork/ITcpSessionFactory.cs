
namespace Caesura.LibNetwork
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    
    public interface ITcpSessionFactory : IDisposable
    {
        ITcpSession AcceptTcpConnection(CancellationToken token);
        Task<ITcpSession> Connect(string host, int port);
        void Start();
        void Stop();
    }
}
