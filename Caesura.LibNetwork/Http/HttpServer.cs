
namespace Caesura.LibNetwork.Http
{
    using System;
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
        private ConcurrentDictionary<Guid, Task> SessionTasks;
        
        public event Func<IHttpSession, Task> OnNewConnection;
        public event Func<Exception, Task> OnUnhandledException;
        public event Func<Exception, Task> OnSessionException;
        public event Func<int, Task> OnSocketException;
        
        public HttpServer(LibNetworkConfig config)
        {
            Config         = config;
            SessionFactory = config.Factories.TcpSessionFactoryFactory(config);
            Sessions       = new ConcurrentDictionary<Guid, IHttpSession>();
            SessionTasks   = new ConcurrentDictionary<Guid, Task>();
            
            OnNewConnection      = delegate { return Task.CompletedTask; };
            OnUnhandledException = delegate { return Task.CompletedTask; };
            OnSessionException   = delegate { return Task.CompletedTask; };
            OnSocketException    = delegate { return Task.CompletedTask; };
        }
        
        public async Task StartAsync()
        {
            Config.DebugLog($"Starting HTTP server for {Config.IP} at port {Config.Port}.");
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
            Config.DebugLog($"Stopping HTTP server for {Config.IP} at port {Config.Port}.");
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
            
            if (!Config.Http.ThreadPerConnection)
            {
                Config.DebugLog("Stopping all sessions.");
                foreach (var (id, session) in Sessions)
                {
                    session.Close();
                }
            }
        }
        
        public Task<IHttpSession> SendRequest(string host, int port) => 
            SendRequestInternal(host, port, null);
        
        public Task<IHttpSession> SendRequest(string host, int port, IHttpRequest request) =>
            SendRequestInternal(host, port, request);
        
        private async Task<IHttpSession> SendRequestInternal(string host, int port, IHttpRequest? request)
        {
            Config.DebugLog($"Sending request: {request?.ToString() ?? "<NULL>"}");
            ValidateRuntime();
            
            IHttpSession? http_session = null;
            try
            {
                var tcp_session = await SessionFactory.Connect(host, port);
                http_session = await AddSession(tcp_session, Canceller!.Token);
                
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
                // await OnUnhandledException.Invoke(e);
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
            var delay = Config.Http.ConnectionLoopMillisecondDelayInterval;
            while (!token.IsCancellationRequested)
            {
                if (delay > 0)
                {
                    // If the delay is under a single Windows clock
                    // resolution cycle (15ms) then skip setting up
                    // the try/catch frame for better perforomance.
                    // Otherwise the delay could be a very long time,
                    // and we need to use the CancellationToken, thus
                    // having to catch a TaskCancellationException.
                    if (delay < 16)
                    {
                        await Task.Delay(delay);
                    }
                    else
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
                
                if (!Config.Http.ThreadPerConnection)
                {
                    await PulseSessions(token);
                }
                
                if (Sessions.Count > Config.MaxConnections || !SessionFactory.Pending())
                {
                    continue;
                }
                
                ITcpSession? tcp_session = null;
                IHttpSession? http_session = null;
                try
                {
                    tcp_session = await SessionFactory.AcceptTcpConnection(token);
                    Config.DebugLog($"Got TCP session for {Config.IP} at port {Config.Port}.");
                    if (token.IsCancellationRequested)
                    {
                        tcp_session.Close();
                    }
                    else
                    {
                        await AddSession(tcp_session, token);
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
        
        private async Task StartSession(IHttpSession session, int delay, CancellationToken token)
        {
            try
            {
                await session.Start(delay, token);
            }
            catch (Exception e)
            {
                await OnSessionException.Invoke(e);
            }
            finally
            {
                RemoveSession(session);
                
                // Repeatedly attempt to remove the session, just in case
                // HttpSession.Start() threw before it was added to the
                // SessionTasks collection (see below in `AddSession()`).
                // It should take around 3 seconds (30 tries of 100 ms delay)
                // before it gives up.
                var remove_success     = false;
                var remove_attempts    = 30; // arbitrary magic numbers
                const int remove_delay = 100;
                while (!remove_success && !token.IsCancellationRequested)
                {
                    remove_success = SessionTasks.TryRemove(session.Id, out _);
                    
                    if (!remove_success)
                    {
                        remove_attempts--;
                        if (remove_attempts <= 0)
                        {
                            var err = $"Could not remove session {session.Id} task from internal session task collection.";
                            await OnSessionException.Invoke(new Exception(err));
                            break;
                        }
                        
                        try
                        {
                            await Task.Delay(remove_delay, token);
                        }
                        catch (TaskCanceledException)
                        {
                            break;
                        }
                    }
                }
            }
        }
        
        private Task<IHttpSession> AddSession(ITcpSession tcp_session, CancellationToken token)
        {
            var http_session = Config.Http.Factories.HttpSessionFactory(Config, tcp_session, Canceller!.Token);
            return AddSession(http_session, token);
        }
        
        private async Task<IHttpSession> AddSession(IHttpSession http_session, CancellationToken token)
        {
            var session = Sessions.GetOrAdd(http_session.Id, http_session);
            
            if (Config.Http.ThreadPerConnection)
            {
                var task = Task.Run(async () => 
                    await StartSession(http_session, Config.Http.ConnectionLoopMillisecondDelayInterval, token)
                );
                // `HttpSession.Start()` might throw before we get to this line,
                // so `StartSession()` will run in a loop trying to remove it
                // for a little while (see comments above.)
                SessionTasks.TryAdd(http_session.Id, task);
            }
            
            await OnNewConnection.Invoke(http_session);
            return session;
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
