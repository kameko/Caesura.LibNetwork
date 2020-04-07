
namespace Caesura.LibNetwork
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    
    public class HttpServer : IHttpServer
    {
        private LibNetworkConfig Config;
        private CancellationTokenSource? Canceller;
        private TcpListener Listener;
        private ConcurrentDictionary<Guid, TcpSession> Sessions;
        
        public event Func<HttpRequest, HttpResponseSession, Task> OnGET;
        public event Func<HttpRequest, HttpResponseSession, Task> OnDELETE;
        public event Func<HttpRequest, HttpResponseSession, Task> OnPUT;
        public event Func<HttpRequest, HttpResponseSession, Task> OnPOST;
        public event Func<HttpRequest, HttpResponseSession, Task> OnPATCH;
        public event Func<HttpRequest, HttpResponseSession, Task> OnHEAD;
        public event Func<HttpRequest, HttpResponseSession, Task> OnTRACE;
        public event Func<HttpRequest, HttpResponseSession, Task> OnOPTIONS;
        public event Func<HttpRequest, HttpResponseSession, Task> OnCONNECT;
        public event Func<HttpRequest, HttpResponseSession, Task> OnAnyRequest;
        public event Func<HttpRequest, HttpResponseSession, Task> OnUnknownRequest;
        public event Func<HttpRequest, HttpResponseSession, Task> OnInvalidRequest;
        public event Action<Exception> OnUnhandledException;
        public event Action<int> OnSocketException;
        
        public HttpServer(LibNetworkConfig config)
        {
            Config    = config;
            Listener  = new TcpListener(config.IP, config.Port);
            Sessions  = new ConcurrentDictionary<Guid, TcpSession>();
            
            OnGET     = delegate { return Task.CompletedTask; };
            OnDELETE  = delegate { return Task.CompletedTask; };
            OnPUT     = delegate { return Task.CompletedTask; };
            OnPOST    = delegate { return Task.CompletedTask; };
            OnPATCH   = delegate { return Task.CompletedTask; };
            OnHEAD    = delegate { return Task.CompletedTask; };
            OnTRACE   = delegate { return Task.CompletedTask; };
            OnOPTIONS = delegate { return Task.CompletedTask; };
            OnCONNECT = delegate { return Task.CompletedTask; };
            
            OnAnyRequest     = delegate { return Task.CompletedTask; };
            OnUnknownRequest = delegate { return Task.CompletedTask; };
            OnInvalidRequest = delegate { return Task.CompletedTask; };
            
            OnUnhandledException = delegate { };
            OnSocketException    = delegate { };
        }
        
        public async Task EstablishConnection(HttpRequest request)
        {
            ValidateRuntime();
            
            TcpSession? session = null;
            try
            {
                var client = new TcpClient();
                session = new TcpSession(client, Config.ConnectionTimeoutTicks);
                Sessions.GetOrAdd(session.Id, session);
                
                await client.ConnectAsync(request.Resource.Host, request.Resource.Port);
                
                var bytes = request.ToBytes();
                var sent  = await client.Client.SendAsync(bytes, SocketFlags.Broadcast, Canceller!.Token);
                if (sent < bytes.Length)
                {
                    throw new UnreliableConnectionException(
                          $"Attempted to send {bytes.Length} bytes to {request.Resource.ToString()} "
                        + $"but only sent {sent}."
                    );
                }
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
        
        public async Task StartAsync()
        {
            ValidateStart();
            
            Listener.Start();
            await GetAllSubsystemTasks();
        }
        
        public void Start()
        {
            ValidateStart();
            
            Listener.Start();
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
            
            Listener.Stop();
            Canceller.Cancel();
            
            foreach (var session_kvp in Sessions)
            {
                session_kvp.Value.Client.Close();
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
                    var client = Listener.AcceptTcpClient();
                    var session = new TcpSession(client, Config.ConnectionTimeoutTicks);
                    
                    Sessions.GetOrAdd(session.Id, session);
                }
                catch (SocketException se)
                {
                    OnSocketException(se.ErrorCode);
                }
                catch (Exception e)
                {
                    OnUnhandledException(e);
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
                    if (session_kvp.Value.Client.GetStream().DataAvailable)
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
        
        private async Task HandleSessionSafely(TcpSession session)
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
                OnUnhandledException(e);
                throw;
            }
        }
        
        private async Task HandleSession(TcpSession session)
        {
            ValidateRuntime();
            
            var token            = Canceller!.Token;
            //var stream           = session.Client.GetStream();
            //var request          = NetworkSerialization.GetRequest(token, Config, stream);
            var request          = NetworkSerialization.DeserializeHttpRequest(session.Reader, Config, token);
            var response_session = new HttpResponseSession(token, session);
            
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
                    _                       => OnUnknownRequest(request, response_session)
                });
                await OnAnyRequest(request, response_session);
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
