
namespace Caesura.LibNetwork.Http
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Net.Sockets;
    
    public class HttpServer : IHttpServer
    {
        private LibNetworkConfig Config;
        private CancellationTokenSource? Canceller;
        private ITcpSessionFactory SessionFactory;
        private ConcurrentDictionary<Guid, ITcpSession> Sessions;
        
        public event Func<IHttpRequest, HttpResponseSession, Task> OnGET;
        public event Func<IHttpRequest, HttpResponseSession, Task> OnDELETE;
        public event Func<IHttpRequest, HttpResponseSession, Task> OnPUT;
        public event Func<IHttpRequest, HttpResponseSession, Task> OnPOST;
        public event Func<IHttpRequest, HttpResponseSession, Task> OnPATCH;
        public event Func<IHttpRequest, HttpResponseSession, Task> OnHEAD;
        public event Func<IHttpRequest, HttpResponseSession, Task> OnTRACE;
        public event Func<IHttpRequest, HttpResponseSession, Task> OnOPTIONS;
        public event Func<IHttpRequest, HttpResponseSession, Task> OnCONNECT;
        public event Func<IHttpRequest, HttpResponseSession, Task> OnAnyValidRequest;
        public event Func<IHttpRequest, HttpResponseSession, Task> OnInvalidRequest;
        public event Func<Exception, Task> OnUnhandledException;
        public event Func<int, Task> OnSocketException;
        
        public HttpServer(LibNetworkConfig config)
        {
            Config         = config;
            SessionFactory = config.Factories.TcpSessionFactoryFactory(config);
            Sessions       = new ConcurrentDictionary<Guid, ITcpSession>();
            
            OnGET     = delegate { return Task.CompletedTask; };
            OnDELETE  = delegate { return Task.CompletedTask; };
            OnPUT     = delegate { return Task.CompletedTask; };
            OnPOST    = delegate { return Task.CompletedTask; };
            OnPATCH   = delegate { return Task.CompletedTask; };
            OnHEAD    = delegate { return Task.CompletedTask; };
            OnTRACE   = delegate { return Task.CompletedTask; };
            OnOPTIONS = delegate { return Task.CompletedTask; };
            OnCONNECT = delegate { return Task.CompletedTask; };
            
            OnAnyValidRequest = delegate { return Task.CompletedTask; };
            OnInvalidRequest  = delegate { return Task.CompletedTask; };
            
            OnUnhandledException = delegate { return Task.CompletedTask; };
            OnSocketException    = delegate { return Task.CompletedTask; };
        }
        
        public async Task StartAsync()
        {
            ValidateStart();
            
            SessionFactory.Start();
            await GetAllSubsystemTasks();
        }
        
        public void Start()
        {
            ValidateStart();
            
            SessionFactory.Start();
            var task = GetAllSubsystemTasks();
            
            while (!Canceller!.IsCancellationRequested)
            {
                Thread.Sleep(15);
                if (task.IsFaulted)
                {
                    Stop();
                    throw task.Exception!;
                }
            }
        }
        
        public void Stop()
        {
            if (Canceller is null)
            {
                throw new InvalidOperationException("HTTP server has not been started yet.");
            }
            else if (Canceller.IsCancellationRequested)
            {
                throw new InvalidOperationException("HTTP server has already been cancelled.");
            }
            
            SessionFactory.Stop();
            Canceller.Cancel();
            
            foreach (var session_kvp in Sessions)
            {
                session_kvp.Value.Close();
            }
        }
        
        public Task SendRequest(IHttpRequest request) => 
            SendRequest(request.Resource.Host, request.Resource.Port, request);
        
        public async Task SendRequest(string host, int port, IHttpRequest request)
        {
            ValidateRuntime();
            
            ITcpSession? session = null;
            try
            {
                session = await SessionFactory.Connect(host, port);
                Sessions.GetOrAdd(session.Id, session);
                var http = request.ToHttp();
                await session.Write(http, Canceller!.Token);
            }
            catch (Exception)
            {
                if (!(session is null))
                {
                    Sessions.Remove(session.Id, out _);
                    session.Close();
                }
                
                throw;
            }
        }
        
        private Task GetAllSubsystemTasks()
        {
            return Task.WhenAll(
                ConnectionWaiter(),
                InactiveSessionDetector(),
                SessionHandler()
            );
        }
        
        private void ValidateStart()
        {
            if (Canceller is null)
            {
                Canceller = new CancellationTokenSource();
            }
            ValidateRuntime();
        }
        
        private void ValidateRuntime()
        {
            if (Canceller!.IsCancellationRequested)
            {
                throw new InvalidOperationException("HTTP server already cancelled.");
            }
            else
            {
                throw new InvalidOperationException("HTTP server already running.");
            }
        }
        
        private async Task ConnectionWaiter()
        {
            ValidateRuntime();
            var token = Canceller!.Token;
            while (!token.IsCancellationRequested)
            {
                if (Sessions.Count > Config.MaxConnections)
                {
                    await Task.Delay(15);
                    continue;
                }
                
                try
                {
                    //var client = Listener.AcceptTcpClient();
                    //var session = new TcpSession(client, Config.ConnectionTimeoutTicks);
                    var session = SessionFactory.AcceptTcpConnection();
                    
                    Sessions.GetOrAdd(session.Id, session);
                }
                catch (SocketException se)
                {
                    await OnSocketException(se.ErrorCode);
                }
                catch (Exception e)
                {
                    await OnUnhandledException(e);
                    throw;
                }
            }
        }
        
        private async Task InactiveSessionDetector()
        {
            ValidateRuntime();
            var token = Canceller!.Token;
            while (!token.IsCancellationRequested)
            {
                foreach (var session_kvp in Sessions)
                {
                    if (session_kvp.Value.State == TcpSessionState.Closed)
                    {
                        Sessions.Remove(session_kvp.Key, out _);
                    }
                }
                
                await Task.Delay(1_000, token);
            }
        }
        
        private async Task SessionHandler()
        {
            ValidateRuntime();
            var token = Canceller!.Token;
            while (!token.IsCancellationRequested)
            {
                foreach (var session_kvp in Sessions)
                {
                    if (session_kvp.Value.DataAvailable)
                    {
                        session_kvp.Value.ResetTicks();
                        await HandleSessionSafely(session_kvp.Value);
                    }
                    else
                    {
                        session_kvp.Value.TickDown();
                    }
                }
                
                await Task.Delay(1);
            }
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
                Sessions.Remove(session.Id, out _);
                session.Close();
                await OnUnhandledException(e);
                throw;
            }
        }
        
        private async Task HandleSession(ITcpSession session)
        {
            ValidateRuntime();
            
            var token            = Canceller!.Token;
            var request          = Config.Http.Factories.HttpRequestFactory(Config, session.Output, token);
            var response_session = new HttpResponseSession(session, token);
            
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
                    _                       => throw new InvalidOperationException($"Unrecognized request: {request.Kind}."),
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
            Stop();
        }
    }
}
