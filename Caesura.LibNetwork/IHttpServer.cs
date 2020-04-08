
namespace Caesura.LibNetwork
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
        event Func<IHttpRequest, HttpResponseSession, Task> OnAnyRequest;
        event Func<IHttpRequest, HttpResponseSession, Task> OnUnknownRequest;
        event Func<IHttpRequest, HttpResponseSession, Task> OnInvalidRequest;
        event Action<Exception> OnUnhandledException;
        event Action<int> OnSocketException;
        
        Task StartAsync();
        void Start();
        void Stop();
    }
}
