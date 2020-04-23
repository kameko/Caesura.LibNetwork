
namespace Caesura.LibNetwork.Http
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Net.Sockets;
    
    // https://en.wikipedia.org/wiki/Hypertext_Transfer_Protocol 
    // https://github.com/App-vNext/Polly/wiki/Transient-fault-handling-and-proactive-resilience-engineering 
    
    public class HttpServer : IHttpServer
    {
        private LibNetworkConfig Config;
        private CancellationTokenSource? Canceller;
        private ITcpSessionFactory SessionFactory;
        private ConcurrentDictionary<Guid, IHttpSession> Sessions;
        
        public event Func<Exception, Task> OnUnhandledException;
        public event Func<int, Task> OnSocketException;
        
        public HttpServer(LibNetworkConfig config)
        {
            Config         = config;
            SessionFactory = config.Factories.TcpSessionFactoryFactory(config);
            Sessions       = new ConcurrentDictionary<Guid, IHttpSession>();
            
            OnUnhandledException = delegate { return Task.CompletedTask; };
            OnSocketException    = delegate { return Task.CompletedTask; };
        }
        
        public async Task StartAsync()
        {
            ValidateStart();
            
            SessionFactory.Start();
            await StartAllSubsystemTasks();
        }
        
        public void Start()
        {
            Task.Run(StartAsync);
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
            
            foreach (var (id, session) in Sessions)
            {
                session.Close();
            }
        }
        
        public Task<IHttpSession> SendRequest(string host, int port) => 
            SendRequestInternal(host, port, null);
        
        public Task<IHttpSession> SendRequest(string host, int port, IHttpRequest request) =>
            SendRequestInternal(host, port, request);
        
        private async Task<IHttpSession> SendRequestInternal(string host, int port, IHttpRequest? request)
        {
            ValidateRuntime();
            
            IHttpSession? http_session = null;
            try
            {
                var tcp_session = await SessionFactory.Connect(host, port);
                http_session = AddSession(tcp_session);
                
                if (!(request is null))
                {
                    var http = request.ToHttp();
                    await tcp_session.Write(http, Canceller!.Token);
                }
                
                return http_session;
            }
            catch (Exception)
            {
                RemoveSessionAndClose(http_session);
                
                throw;
            }
        }
        
        private Task StartAllSubsystemTasks()
        {
            return Task.Run(ConnectionHandler);
        }
        
        private void ValidateStart()
        {
            if (Canceller is null)
            {
                Canceller = new CancellationTokenSource();
            }
            if (Canceller.IsCancellationRequested)
            {
                Canceller = new CancellationTokenSource();
            }
        }
        
        private void ValidateRuntime()
        {
            if (Canceller is null)
            {
                throw new InvalidOperationException("HTTP server not started");
            }
            else if (Canceller.IsCancellationRequested)
            {
                throw new InvalidOperationException("HTTP server has been cancelled.");
            }
        }
        
        private async Task ConnectionHandler()
        {
            ValidateRuntime();
            
            var token = Canceller!.Token;
            while (!token.IsCancellationRequested)
            {
                if (Config.Http.ConnectionLoopMillisecondDelayInterval > 0)
                {
                    await Task.Delay(Config.Http.ConnectionLoopMillisecondDelayInterval);
                }
                
                await PulseSessions(token);
                
                if (Sessions.Count > Config.MaxConnections || !SessionFactory.Pending())
                {
                    continue;
                }
                
                ITcpSession? tcp_session = null;
                IHttpSession? http_session = null;
                try
                {
                    tcp_session = await SessionFactory.AcceptTcpConnection(token);
                    if (token.IsCancellationRequested)
                    {
                        tcp_session.Close();
                    }
                    else
                    {
                        http_session = AddSession(tcp_session);
                    }
                }
                catch (SocketException se)
                {
                    OnAnyException();
                    
                    await OnSocketException(se.ErrorCode);
                }
                catch (Exception e)
                {
                    OnAnyException();
                    
                    await OnUnhandledException(e);
                    throw;
                }
                
                void OnAnyException()
                {
                    if (!(http_session is null))
                    {
                        RemoveSessionAndClose(http_session);
                    }
                    else if (!(tcp_session is null))
                    {
                        tcp_session.Close();
                    }
                }
            }
        }
        
        private async Task PulseSessions(CancellationToken token)
        {
            foreach (var (id, session) in Sessions)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }
                
                await session.Pulse();
                
                if (session.Closed)
                {
                    RemoveSession(id);
                }
            }
        }
        
        private IHttpSession AddSession(ITcpSession tcp_session)
        {
            var http_session = Config.Http.Factories.HttpSessionFactory(Config, tcp_session, Canceller!.Token);
            return AddSession(http_session);
        }
        
        private IHttpSession AddSession(IHttpSession http_session)
        {
            var ret = Sessions.GetOrAdd(http_session.Id, http_session);
            return ret;
        }
        
        private IHttpSession? RemoveSession(IHttpSession session) => RemoveSession(session.Id);
        
        private IHttpSession? RemoveSession(Guid id)
        {
            var r = Sessions.TryRemove(id, out var ret);
            return r ? ret : null;
        }
        
        private bool RemoveSessionAndClose(IHttpSession? session)
        {
            if (session is null)
            {
                return false;
            }
            
            var s = RemoveSession(session);
            if (s is null)
            {
                return false;
            }
            else
            {
                s.Close();
                return true;
            }
        }
        
        public void Dispose()
        {
            Stop();
        }
    }
}
