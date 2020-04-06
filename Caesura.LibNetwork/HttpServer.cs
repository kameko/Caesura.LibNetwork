
namespace Caesura.LibNetwork
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Net;
    using System.Net.Http;
    using System.Net.Sockets;
    
    // TODO: events for GET, DELETE, POST, PUT and PATCH here.
    // Also an event that triggers for all of them. And make
    // them async.
    // Also an event for unrecognized request names. Not an
    // outright error, but something to be informed of.
    // Also something to handle exceptions, both critical and
    // non-critical (network errors).
    
    public class HttpServer : IHttpServer
    {
        private LibNetworkConfig Config;
        private CancellationTokenSource? Canceller;
        private TcpListener Listener;
        private List<TcpSession> Sessions;
        
        public event Func<HttpRequest, HttpResponseSession, Task> OnGET;
        
        public HttpServer(LibNetworkConfig config, IPAddress ip, int port)
        {
            var nport = port <= 0 ? HttpServers.DefaultIpAddress : port;
            Config    = config;
            Listener  = new TcpListener(ip, nport);
            Sessions  = new List<TcpSession>();
            
            OnGET = delegate { return Task.CompletedTask; };
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
                .Run(() =>
                {
                    ValidateStart();
                    Listener.Start();
                    ConnectionWaiter();
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
        
        private Task ConnectionWaiter()
        {
            var cancelled = Canceller?.IsCancellationRequested ?? true;
            while (!cancelled)
            {
                if (Sessions.Count > Config.MaxConnections)
                {
                    Thread.Sleep(15);
                    continue;
                }
                
                try
                {
                    var client = Listener.AcceptTcpClient();
                    var session = new TcpSession(client);
                    
                    Sessions.Add(session);
                    Task.Run(async () =>
                    {
                        await HandleSession(session);
                    })
                    .ContinueWith(t =>
                    {
                        if (t.IsFaulted)
                        {
                            // TODO: handle error
                        }
                        session.Client.Close();
                        Sessions.Remove(session);
                    });
                }
                catch (SocketException se)
                {
                    // TODO: MSDN documentation for TcpListener.AcceptTcpClient():
                    //   ``Use the ErrorCode property to obtain the specific error code.
                    //     When you have obtained this code, you can refer to the Windows
                    //     Sockets version 2 API error code documentation for a detailed
                    //     description of the error. ,,
                    // 
                    throw new NotImplementedException("IMPLEMENT SOCKETEXCEPTION", se);
                }
            }
            return Task.CompletedTask;
        }
        
        private async Task HandleSession(TcpSession session)
        {
            var token   = Canceller?.Token ?? (new CancellationTokenSource(Config.DefaultTimeoutMilliseconds)).Token;
            var stream  = session.Client.GetStream();
            var request = NetworkSerialization.GetRequest(token, Config, stream);
            
            if (request.IsValid)
            {
                var response_session = new HttpResponseSession(token, stream);
                await (request.Kind switch
                {
                    HttpRequestKind.GET => OnGET(request, response_session),
                    _                   => throw new NotImplementedException(),
                });
            }
            else
            {
                // TODO: call an event
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
