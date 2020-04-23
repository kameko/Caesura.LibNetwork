
namespace Caesura.LibNetwork.Http
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    
    public interface IHttpSession : IDisposable
    {
        string Name { get; set; }
        Guid Id { get; }
        TimeSpan Timeout { get; set; }
        ITcpSession TcpSession { get; }
        bool Closed { get; }
        
        event Func<IHttpRequest, IHttpSession, Task> OnGET;
        event Func<IHttpRequest, IHttpSession, Task> OnDELETE;
        event Func<IHttpRequest, IHttpSession, Task> OnPUT;
        event Func<IHttpRequest, IHttpSession, Task> OnPOST;
        
        event Func<IHttpRequest, IHttpSession, Task> OnHEAD;
        event Func<IHttpRequest, IHttpSession, Task> OnPATCH;
        event Func<IHttpRequest, IHttpSession, Task> OnTRACE;
        event Func<IHttpRequest, IHttpSession, Task> OnOPTIONS;
        event Func<IHttpRequest, IHttpSession, Task> OnCONNECT;
        
        event Func<IHttpRequest, IHttpSession, Task> OnAnyValidRequest;
        event Func<IHttpRequest, IHttpSession, Task> OnInvalidRequest;
        
        event Func<IHttpSession, Task> OnTimeoutDisconnect;
        event Func<Exception, Task> OnUnhandledException;
        
        Task Respond(IHttpResponse response);
        Task Start(CancellationToken token);
        Task Start(int delay, CancellationToken token);
        Task Pulse();
        void Stop();
        void Close();
    }
}
