
namespace Caesura.LibNetwork.Http
{
    using System;
    using System.Threading.Tasks;
    
    public interface IHttpServer : IDisposable
    {
        event Func<Exception, Task> OnUnhandledException;
        event Func<int, Task> OnSocketException;
        
        Task StartAsync();
        void Start();
        void Stop();
        Task<ITcpSession> SendRequest(string host, int port);
        Task<ITcpSession> SendRequest(string host, int port, IHttpRequest? request);
    }
}
