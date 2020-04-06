
namespace Caesura.LibNetwork
{
    using System;
    using System.Threading.Tasks;
    
    public interface IHttpServer : IDisposable
    {
        event Func<HttpRequest, HttpResponseSession, Task> OnGET;
        event Func<HttpRequest, HttpResponseSession, Task> OnDELETE;
        event Func<HttpRequest, HttpResponseSession, Task> OnPUT;
        event Func<HttpRequest, HttpResponseSession, Task> OnPOST;
        event Func<HttpRequest, HttpResponseSession, Task> OnPATCH;
        event Func<HttpRequest, HttpResponseSession, Task> OnHEAD;
        event Func<HttpRequest, HttpResponseSession, Task> OnTRACE;
        event Func<HttpRequest, HttpResponseSession, Task> OnOPTIONS;
        event Func<HttpRequest, HttpResponseSession, Task> OnCONNECT;
        event Func<HttpRequest, HttpResponseSession, Task> OnAnyRequest;
        event Func<HttpRequest, HttpResponseSession, Task> OnUnknownRequest;
        event Func<HttpRequest, HttpResponseSession, Task> OnInvalidRequest;
        event Action<Exception> OnUnhandledException;
        event Action<int> OnSocketException;
        
        Task StartAsync();
        void Start();
        void Stop();
    }
}
