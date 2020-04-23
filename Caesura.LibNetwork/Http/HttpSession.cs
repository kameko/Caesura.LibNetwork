
namespace Caesura.LibNetwork.Http
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    
    public class HttpSession : IHttpSession
    {
        private LibNetworkConfig _config;
        private ITcpSession _session;
        private CancellationTokenSource _canceller;
        private CancellationToken _token;
        private bool _closed;
        
        public string Name { get; set; }
        public Guid Id { get; protected set; }
        public ITcpSession TcpSession => _session;
        public bool Closed => _closed;
        
        public event Func<IHttpRequest, HttpSession, Task> OnGET;
        public event Func<IHttpRequest, HttpSession, Task> OnDELETE;
        public event Func<IHttpRequest, HttpSession, Task> OnPUT;
        public event Func<IHttpRequest, HttpSession, Task> OnPOST;
        
        public event Func<IHttpRequest, HttpSession, Task> OnHEAD;
        public event Func<IHttpRequest, HttpSession, Task> OnPATCH;
        public event Func<IHttpRequest, HttpSession, Task> OnTRACE;
        public event Func<IHttpRequest, HttpSession, Task> OnOPTIONS;
        public event Func<IHttpRequest, HttpSession, Task> OnCONNECT;
        
        public event Func<IHttpRequest, HttpSession, Task> OnAnyValidRequest;
        public event Func<IHttpRequest, HttpSession, Task> OnInvalidRequest;
        public event Func<Exception, Task> OnUnhandledException;
        
        internal HttpSession(LibNetworkConfig config, ITcpSession session, CancellationToken token)
        {
            _config    = config;
            _session   = session;
            _canceller = new CancellationTokenSource();
            _token     = token;
            _closed    = false;
            
            Name       = nameof(HttpSession);
            Id         = Guid.NewGuid();
            
            OnGET      = delegate { return Task.CompletedTask; };
            OnDELETE   = delegate { return Task.CompletedTask; };
            OnPUT      = delegate { return Task.CompletedTask; };
            OnPOST     = delegate { return Task.CompletedTask; };
            
            OnHEAD     = delegate { return Task.CompletedTask; };
            OnPATCH    = delegate { return Task.CompletedTask; };
            OnTRACE    = delegate { return Task.CompletedTask; };
            OnOPTIONS  = delegate { return Task.CompletedTask; };
            OnCONNECT  = delegate { return Task.CompletedTask; };
            
            OnAnyValidRequest = delegate { return Task.CompletedTask; };
            OnInvalidRequest  = delegate { return Task.CompletedTask; };
            
            OnUnhandledException = delegate { return Task.CompletedTask; };
        }
        
        public async Task Respond(IHttpResponse response)
        {
            if (!response.IsValid)
            {
                throw new InvalidHttpResponseException(response.Validation);
            }
            if (_session.State == TcpSessionState.Closed)
            {
                throw new TcpSessionNotActiveException();
            }
            
            var http = response.ToHttp();
            await _session.Write(http, _token);
        }
        
        public Task Start(CancellationToken token) => Start(-1, token);
        
        public Task Start(int delay, CancellationToken token)
        {
            return Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    await Pulse();
                    
                    if (delay > 0)
                    {
                        await Task.Delay(delay, token);
                    }
                }
                
                Close();
            });
        }
        
        public async Task Pulse()
        {
            if (_session.DataAvailable)
            {
                await HandleSessionSafely(_session);
            }
        }
        
        public void Stop() => Close();
        
        public void Close()
        {
            _closed = true;
            _canceller.Cancel();
            _session.Close();
        }
        
        private async Task HandleSessionSafely(ITcpSession session)
        {
            try
            {
                if (session.State == TcpSessionState.Closed)
                {
                    throw new TcpSessionNotActiveException("Session is no longer active.");
                }
                
                await HandleSession(session);
            }
            catch (Exception e)
            {
                Close();
                await OnUnhandledException(e);
                throw;
            }
        }
        
        private async Task HandleSession(ITcpSession session)
        {
            var request          = _config.Http.Factories.HttpRequestFactory(_config, session.Output, _canceller.Token);
            var response_session = this;
            
            if (request.IsValid)
            {
                await (request.Kind switch
                {
                    HttpRequestKind.GET     => OnGET(request, response_session),
                    HttpRequestKind.DELETE  => OnDELETE(request, response_session),
                    HttpRequestKind.PUT     => OnPUT(request, response_session),
                    HttpRequestKind.POST    => OnPOST(request, response_session),
                    HttpRequestKind.PATCH   => OnPATCH(request, response_session),
                    HttpRequestKind.HEAD    => OnHEAD(request, response_session),
                    HttpRequestKind.TRACE   => OnTRACE(request, response_session),
                    HttpRequestKind.OPTIONS => OnOPTIONS(request, response_session),
                    HttpRequestKind.CONNECT => OnCONNECT(request, response_session),
                    _ => throw new InvalidOperationException(
                            $"Should not get here! Unrecognized request: {request.Kind}."
                        ),
                });
                await OnAnyValidRequest(request, response_session);
            }
            else
            {
                await OnInvalidRequest(request, response_session);
            }
        }
        
        public void Dispose()
        {
            Close();
        }
    }
}
