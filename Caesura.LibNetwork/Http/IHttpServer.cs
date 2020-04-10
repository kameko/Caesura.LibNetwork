
namespace Caesura.LibNetwork.Http
{
    using System;
    using System.Threading.Tasks;
    
    public interface IHttpServer : IDisposable
    {
        event Func<IHttpRequest, HttpResponseSession, Task> OnGET;
        event Func<IHttpRequest, HttpResponseSession, Task> OnDELETE;
        event Func<IHttpRequest, HttpResponseSession, Task> OnPUT;
        event Func<IHttpRequest, HttpResponseSession, Task> OnPOST;
        event Func<IHttpRequest, HttpResponseSession, Task> OnPATCH;
        event Func<IHttpRequest, HttpResponseSession, Task> OnHEAD;
        event Func<IHttpRequest, HttpResponseSession, Task> OnTRACE;
        event Func<IHttpRequest, HttpResponseSession, Task> OnOPTIONS;
        event Func<IHttpRequest, HttpResponseSession, Task> OnCONNECT;
        event Func<IHttpRequest, HttpResponseSession, Task> OnAnyValidRequest;
        event Func<IHttpRequest, HttpResponseSession, Task> OnInvalidRequest;
        event Func<Exception, Task> OnUnhandledException;
        event Func<int, Task> OnSocketException;
        
        Task StartAsync();
        void Start();
        void Stop();
        Task<ITcpSession> SendRequest(string host, int port);
        Task<ITcpSession> SendRequest(string host, int port, IHttpRequest? request);
    }
}
