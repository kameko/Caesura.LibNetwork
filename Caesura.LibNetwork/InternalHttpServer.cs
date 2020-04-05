
namespace Caesura.LibNetwork
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
    
    internal class InternalHttpServer : IHttpServer
    {
        private LibNetworkConfig Config;
        private CancellationTokenSource? Canceller;
        private TcpListener Listener;
        private List<TcpSession> Sessions;
        
        public InternalHttpServer(LibNetworkConfig config, IPAddress ip, int port)
        {
            var nport = port <= 0 ? HttpServers.DefaultIpAddress : port;
            Config    = config;
            Listener  = new TcpListener(ip, nport);
            Sessions  = new List<TcpSession>();
        }
        
        public InternalHttpServer(LibNetworkConfig config, string ip, int port) : this(config, IPAddress.Parse(ip), port)
        {
            
        }
        
        public InternalHttpServer(LibNetworkConfig config, int port) : this(config, IPAddress.IPv6Loopback, port)
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
                    Task.WaitAll(ConnectionWaiter(), ConnectionHandler());
                })
                .ContinueWith(t => Stop());
        }
        
        public void Start()
        {
            ValidateStart();
            Listener.Start();
            Task.Run(ConnectionWaiter);
            Task.Run(ConnectionHandler);
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
            
            // TODO: TcpListener.Stop() does not close existing connections, this must be done manually.
            Listener.Stop();
            Canceller.Cancel();
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
                }
                catch (SocketException)
                {
                    // TODO: MSDN documentation for TcpListener.AcceptTcpClient():
                    //   ``Use the ErrorCode property to obtain the specific error code.
                    //     When you have obtained this code, you can refer to the Windows
                    //     Sockets version 2 API error code documentation for a detailed
                    //     description of the error. ,,
                    // 
                }
            }
            return Task.CompletedTask;
        }
        
        private Task ConnectionHandler()
        {
            var sessions = new List<TcpSession>(Sessions);
            foreach (var session in sessions)
            {
                HandleSession(session);
            }
            
            return Task.CompletedTask;
        }
        
        private Task HandleSession(TcpSession session)
        {
            var inputStream  = session.Client.GetStream();
            var outputStream = session.Client.GetStream();
            
            return Task.CompletedTask;
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
