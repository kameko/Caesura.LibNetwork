
namespace Caesura.LibNetwork
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Net;
    using System.Net.Sockets;
    
    public class HttpServer : IHttpServer
    {
        private LibNetworkConfig Config;
        private CancellationTokenSource? Canceller;
        private TcpListener Listener;
        private List<TcpSession> Sessions;
        
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
        
        public HttpServer(LibNetworkConfig config, IPAddress ip, int port)
        {
            var nport = port <= 0 ? HttpServers.DefaultIpAddress : port;
            Config    = config;
            Listener  = new TcpListener(ip, nport);
            Sessions  = new List<TcpSession>();
            
            OnGET            = delegate { return Task.CompletedTask; };
            OnDELETE         = delegate { return Task.CompletedTask; };
            OnPUT            = delegate { return Task.CompletedTask; };
            OnPOST           = delegate { return Task.CompletedTask; };
            OnPATCH          = delegate { return Task.CompletedTask; };
            OnHEAD           = delegate { return Task.CompletedTask; };
            OnTRACE          = delegate { return Task.CompletedTask; };
            OnOPTIONS        = delegate { return Task.CompletedTask; };
            OnCONNECT        = delegate { return Task.CompletedTask; };
            OnAnyRequest     = delegate { return Task.CompletedTask; };
            OnUnknownRequest = delegate { return Task.CompletedTask; };
            OnInvalidRequest = delegate { return Task.CompletedTask; };
            
            OnUnhandledException = delegate { };
            OnSocketException    = delegate { };
        }
        
        public HttpServer(LibNetworkConfig config, string ip, int port) : this(config, IPAddress.Parse(ip), port)
        {
            
        }
        
        public HttpServer(LibNetworkConfig config, int port) : this(config, IPAddress.IPv6Loopback, port)
        {
            
        }
        
        public Task StartAsync()
        {
            ValidateStart();
            return Task
                .Run(async () =>
                {
                    ValidateStart();
                    Listener.Start();
                    await ConnectionWaiter();
                })
                .ContinueWith(t =>
                {
                    Stop();
                });
        }
        
        public void Start()
        {
            ValidateStart();
            Listener.Start();
            Task.Run(ConnectionWaiter);
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
            
            var sessions = new List<TcpSession>(Sessions);
            foreach (var session in sessions)
            {
                session.Client.Close();
            }
        }
        
        private void ValidateStart()
        {
            if (Canceller is null)
            {
                Canceller = new CancellationTokenSource();
            }
            else if (Canceller.IsCancellationRequested)
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
            var cancelled = Canceller?.IsCancellationRequested ?? true;
            while (!cancelled)
            {
                if (Sessions.Count > Config.MaxConnections)
                {
                    Thread.Sleep(15);
                    continue;
                }
                
                TcpSession? session = null;
                try
                {
                    var client = Listener.AcceptTcpClient();
                    session = new TcpSession(client);
                    
                    Sessions.Add(session);
                    await HandleSession(session);
                }
                catch (SocketException se)
                {
                    OnSocketException(se.ErrorCode);
                }
                catch (Exception e)
                {
                    OnUnhandledException(e);
                }
                finally
                {
                    if (!(session is null))
                    {
                        session.Client.Close();
                        Sessions.Remove(session);
                    }
                }
            }
        }
        
        private async Task HandleSession(TcpSession session)
        {
            if (Canceller is null)
            {
                throw new InvalidOperationException("Http server has not been started yet.");
            }
            if (Canceller.IsCancellationRequested)
            {
                throw new InvalidOperationException("Http server has been cancelled.");
            }
            
            var token            = Canceller.Token;
            var stream           = session.Client.GetStream();
            var request          = NetworkSerialization.GetRequest(token, Config, stream);
            var response_session = new HttpResponseSession(token, stream);
            
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
        
        private class TcpSession
        {
            public TcpClient Client;
            
            public TcpSession(TcpClient client)
            {
                Client = client;
            }
        }
    }
}
