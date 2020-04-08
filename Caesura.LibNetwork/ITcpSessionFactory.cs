
namespace Caesura.LibNetwork
{
    public interface ITcpSessionFactory
    {
        ITcpSession AcceptTcpConnection();
        void Start();
        void Stop();
    }
}
