
namespace Caesura.LibNetwork
{
    using System.Threading.Tasks;
    
    public interface ITcpSessionFactory
    {
        ITcpSession AcceptTcpConnection();
        Task<ITcpSession> Connect(string host, int port);
        void Start();
        void Stop();
    }
}
