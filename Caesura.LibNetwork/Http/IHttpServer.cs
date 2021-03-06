
namespace Caesura.LibNetwork.Http
{
    using System;
    using System.Threading.Tasks;
    
    public interface IHttpServer : IDisposable
    {
        event Func<IHttpSession, Task> OnNewConnection;
        event Func<Exception, Task> OnUnhandledException;
        event Func<Exception, Task> OnSessionException;
        event Func<int, Task> OnSocketException;
        
        Task StartAsync();
        void Start();
        void Stop();
        Task<IHttpSession> SendRequest(string host, int port);
        Task<IHttpSession> SendRequest(string host, int port, IHttpRequest request);
    }
}
