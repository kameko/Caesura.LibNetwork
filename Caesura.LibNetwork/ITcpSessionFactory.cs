
namespace Caesura.LibNetwork
{
    using System;
    using System.Threading.Tasks;
    
    public interface ITcpSessionFactory : IDisposable
    {
        ITcpSession AcceptTcpConnection();
        Task<ITcpSession> Connect(string host, int port);
        void Start();
        void Stop();
    }
}
