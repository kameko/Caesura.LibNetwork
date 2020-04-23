
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
        
        private TimeSpan last_response_time;
        private TimeSpan last_pulse_time;
        
        public string Name { get; set; }
        public Guid Id { get; protected set; }
        public TimeSpan Timeout { get; set; }
        public ITcpSession TcpSession => _session;
        public bool Closed => _closed;
        
        public event Func<IHttpRequest, IHttpSession, Task> OnGET;
        public event Func<IHttpRequest, IHttpSession, Task> OnDELETE;
        public event Func<IHttpRequest, IHttpSession, Task> OnPUT;
        public event Func<IHttpRequest, IHttpSession, Task> OnPOST;
        
        public event Func<IHttpRequest, IHttpSession, Task> OnHEAD;
        public event Func<IHttpRequest, IHttpSession, Task> OnPATCH;
        public event Func<IHttpRequest, IHttpSession, Task> OnTRACE;
        public event Func<IHttpRequest, IHttpSession, Task> OnOPTIONS;
        public event Func<IHttpRequest, IHttpSession, Task> OnCONNECT;
        
        public event Func<IHttpRequest, IHttpSession, Task> OnAnyValidRequest;
        public event Func<IHttpRequest, IHttpSession, Task> OnInvalidRequest;
        
        public event Func<IHttpSession, Task> OnTimeoutDisconnect;
        public event Func<Exception, Task> OnUnhandledException;
        
        internal HttpSession(LibNetworkConfig config, ITcpSession session, CancellationToken token)
        {
            _config    = config;
            _session   = session;
            _canceller = new CancellationTokenSource();
            _token     = token;
            _closed    = false;
            
            last_response_time = new TimeSpan(DateTime.UtcNow.Ticks);
            last_pulse_time    = last_response_time;
            
            Name       = nameof(HttpSession);
            Id         = Guid.NewGuid();
            Timeout    = config.Http.SessionTimeout;
            
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
            
            OnTimeoutDisconnect  = delegate { return Task.CompletedTask; };
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
                while (!token.IsCancellationRequested && !_closed)
                {
                    await Pulse();
                    
                    if (delay > 0)
                    {
                        try
                        {
                            await Task.Delay(delay, token);
                        }
                        catch (TaskCanceledException)
                        {
                            break;
                        }
                    }
                }
                
                Close();
            });
        }
        
        public async Task Pulse()
        {
            // TODO: make sure all this timeout stuff works
            
            last_pulse_time = new TimeSpan(DateTime.UtcNow.Ticks);
            
            if (_session.DataAvailable)
            {
                last_response_time = new TimeSpan(DateTime.UtcNow.Ticks);
                await HandleSession(_session);
            }
            else
            {
                var delta = new TimeSpan(Math.Abs((last_response_time - last_pulse_time).Ticks));
                if (delta > Timeout)
                {
                    Close();
                    await OnTimeoutDisconnect.Invoke(this);
                }
            }
        }
        
        public void Stop() => Close();
        
        public void Close()
        {
            _closed = true;
            _canceller.Cancel();
            _session.Close();
        }
        
        private async Task HandleSession(ITcpSession session)
        {
            try
            {
                if (session.State == TcpSessionState.Closed)
                {
                    throw new TcpSessionNotActiveException();
                }
                
                await InternalHandleSession(session);
            }
            catch (Exception e)
            {
                Close();
                await OnUnhandledException(e);
                throw;
            }
        }
        
        private async Task InternalHandleSession(ITcpSession session)
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
