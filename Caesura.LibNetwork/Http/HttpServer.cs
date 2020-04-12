
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
        private ConcurrentDictionary<Guid, ITcpSession> Sessions;
        
        public event Func<Exception, Task> OnUnhandledException;
        public event Func<int, Task> OnSocketException;
        
        public HttpServer(LibNetworkConfig config)
        {
            Config         = config;
            SessionFactory = config.Factories.TcpSessionFactoryFactory(config);
            Sessions       = new ConcurrentDictionary<Guid, ITcpSession>();
            
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
            
            foreach (var session_kvp in Sessions)
            {
                session_kvp.Value.Close();
            }
        }
        
        public Task<ITcpSession> SendRequest(string host, int port) => 
            SendRequest(host, port, null!);
        
        public async Task<ITcpSession> SendRequest(string host, int port, IHttpRequest? request)
        {
            ValidateRuntime();
            
            ITcpSession? session = null;
            try
            {
                session = await SessionFactory.Connect(host, port);
                Sessions.GetOrAdd(session.Id, session);
                if (!(request is null))
                {
                    var http = request.ToHttp();
                    await session.Write(http, Canceller!.Token);
                }
                return session;
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
                Task.Run(ConnectionWaiter),
                Task.Run(SessionHandler)
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
            if (Canceller is null)
            {
                throw new InvalidOperationException("HTTP server not started");
            }
            else if (Canceller.IsCancellationRequested)
            {
                throw new InvalidOperationException("HTTP server has been cancelled.");
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
                    var session = SessionFactory.AcceptTcpConnection(token);
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
        
        private async Task SessionHandler()
        {
            ValidateRuntime();
            var token = Canceller!.Token;
            var remove_ids = new List<Guid>();
            while (!token.IsCancellationRequested)
            {
                foreach (var session_kvp in Sessions)
                {
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }
                    
                    session_kvp.Value.Pulse();
                    
                    if (session_kvp.Value.DataAvailable)
                    {
                        session_kvp.Value.ResetTicks();
                        // await HandleSessionSafely(session_kvp.Value);
                    }
                    else
                    {
                        if (session_kvp.Value.State == TcpSessionState.Closed)
                        {
                            remove_ids.Add(session_kvp.Key);
                        }
                    }
                }
                if (remove_ids.Count > 0)
                {
                    foreach (var id in remove_ids)
                    {
                        if (token.IsCancellationRequested)
                        {
                            break;
                        }
                        
                        Sessions.Remove(id, out _);
                    }
                    remove_ids.Clear();
                }
                
                await Task.Delay(1);
            }
        }
        
        public void Dispose()
        {
            Stop();
        }
    }
}
