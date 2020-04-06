
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
        
        public HttpServer(LibNetworkConfig config, IPAddress ip, int port)
        {
            var nport = port <= 0 ? HttpServers.DefaultIpAddress : port;
            Config    = config;
            Listener  = new TcpListener(ip, nport);
            Sessions  = new List<TcpSession>();
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
                    Task.Run(() =>
                    {
                        HandleSession(session);
                    })
                    .ContinueWith(t =>
                    {
                        if (t.IsFaulted)
                        {
                            // TODO: handle error
                        }
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
        
        private Task HandleSession(TcpSession session)
        {
            var sb      = new StringBuilder();
            var input   = session.Client.GetStream();
            var output  = session.Client.GetStream();
            
            var request = GetRequest(input, sb);
            var headers = GetHeaders(input, sb);
            var body    = GetBody(input, sb);
            var message = new HttpMessage(request, headers, body);
            
            if (message.IsValid)
            {
                
                // TODO: invoke callbacks
            }
            
            return Task.CompletedTask;
        }
        
        private HttpRequest GetRequest(NetworkStream stream, StringBuilder sb)
        {
            var request_str = ReadLine(stream, sb, Config.HeaderCharReadLimit);
            var request     = new HttpRequest(request_str);
            return request;
        }
        
        private HttpHeaders GetHeaders(NetworkStream stream, StringBuilder sb)
        {
            var headers = new HttpHeaders();
            
            var line = string.Empty;
            while (true)
            {
                line = ReadLine(stream, sb, Config.HeaderCharReadLimit);
                
                if (line == "\r\n")
                {
                    break;
                }
                
                var header = new HttpHeader(line);
                headers.Add(header);
            }
            
            return headers;
        }
        
        private HttpBody GetBody(NetworkStream stream, StringBuilder sb)
        {
            sb.Clear();
            
            char current_char = '\0';
            int current_int   = 0;
            while (current_int > -1 && current_int < Config.BodyCharReadLimit)
            {
                current_int  = stream.ReadByte();
                current_char = Convert.ToChar(current_int);
                sb.Append(current_char);
            }
            
            var body = new HttpBody(sb.ToString());
            return body;
        }
        
        private string ReadLine(NetworkStream stream, StringBuilder sb, int limit)
        {
            sb.Clear();
            
            char last_char    = '\0';
            char current_char = '\0';
            int current_int   = 0;
            while (current_int > -1 && current_int < limit)
            {
                current_int  = stream.ReadByte();
                current_char = Convert.ToChar(current_int);
                
                sb.Append(current_char);
                
                if (last_char == '\r' && current_char == '\n')
                {
                    break;
                }
                
                last_char = current_char;
            }
            return sb.ToString();
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
