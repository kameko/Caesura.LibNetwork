
namespace Caesura.LibNetwork.Http
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    
    public interface IHttpSession : IDisposable
    {
        event Func<IHttpRequest, HttpSession, Task> OnGET;
        event Func<IHttpRequest, HttpSession, Task> OnDELETE;
        event Func<IHttpRequest, HttpSession, Task> OnPUT;
        event Func<IHttpRequest, HttpSession, Task> OnPOST;
        event Func<IHttpRequest, HttpSession, Task> OnPATCH;
        event Func<IHttpRequest, HttpSession, Task> OnHEAD;
        event Func<IHttpRequest, HttpSession, Task> OnTRACE;
        event Func<IHttpRequest, HttpSession, Task> OnOPTIONS;
        event Func<IHttpRequest, HttpSession, Task> OnCONNECT;
        event Func<IHttpRequest, HttpSession, Task> OnAnyValidRequest;
        event Func<IHttpRequest, HttpSession, Task> OnInvalidRequest;
        event Func<Exception, Task> OnUnhandledException;
        
        Task Respond(IHttpResponse response);
        void Close();
    }
}
