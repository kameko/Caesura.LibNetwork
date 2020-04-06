
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
            var nport = port <= 0 ? HttpServers.DefaultPort : port;
            Config    = config;
            Listener  = new TcpListener(ip, nport);
            Sessions  = new List<TcpSession>();
            
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
        
        public HttpServer(LibNetworkConfig config, string ip, int port) : this(config, IPAddress.Parse(ip), port)
        {
            
        }
        
        public HttpServer(LibNetworkConfig config, int port) : this(config, IPAddress.IPv6Loopback, port)
        {
            
        }
        
        public async Task EstablishConnection(HttpRequest request)
        {
            ValidateStart();
            
            TcpSession? session = null;
            try
            {
                var client = new TcpClient();
                session = new TcpSession(client, Config.ConnectionTimeoutTicks);
                Sessions.Add(session);
                
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
                Sessions.Remove(session!);
                session?.Dispose();
                
                throw;
            }
            
        }
        
        public Task StartAsync()
        {
            ValidateStart();
            return Task
                .Run(async () =>
                {
                    Listener.Start();
                    await Task.WhenAll(
                        ConnectionWaiter(),
                        ConnectionTimeoutDetector()
                    );
                });
        }
        
        public void Start()
        {
            ValidateStart();
            Listener.Start();
            Task.WhenAll(
                ConnectionWaiter(),
                ConnectionTimeoutDetector()
            );
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
        
        private async Task ConnectionTimeoutDetector()
        {
            var cancelled = Canceller?.IsCancellationRequested ?? true;
            while (!cancelled)
            {
                await Task.Delay(1_000);
                
                var sessions = new List<TcpSession>(Sessions);
                foreach (var session in sessions)
                {
                    session.TickDown();
                    if (!session.Active)
                    {
                        Sessions.Remove(session);
                    }
                }
            }
        }
        
        private async Task ConnectionWaiter()
        {
            var cancelled = Canceller?.IsCancellationRequested ?? true;
            while (!cancelled)
            {
                if (Sessions.Count > Config.MaxConnections)
                {
                    await Task.Delay(15);
                    continue;
                }
                
                TcpSession? session = null;
                try
                {
                    var client = Listener.AcceptTcpClient();
                    session = new TcpSession(client, Config.ConnectionTimeoutTicks);
                    
                    Sessions.Add(session);
                    await HandleSession(session);
                }
                catch (SocketException se)
                {
                    if (!(session is null))
                    {
                        session.Client.Close();
                        Sessions.Remove(session);
                    }
                    OnSocketException(se.ErrorCode);
                }
                catch (Exception e)
                {
                    if (!(session is null))
                    {
                        session.Client.Close();
                        Sessions.Remove(session);
                    }
                    OnUnhandledException(e);
                }
            }
        }
        
        // TODO: we need a way to handle back-and-forth communication
        // without killing the client after every request.
        // I think we should run HandleSession in a loop in it's own
        // task and maybe remove the call to it in ConnectionWaiter().
        
        private async Task HandleSession(TcpSession session)
        {
            ValidateStart();
            
            var token            = Canceller!.Token;
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
    }
}
