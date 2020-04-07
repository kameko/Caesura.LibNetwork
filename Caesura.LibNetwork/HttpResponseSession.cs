
namespace Caesura.LibNetwork
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Net.Sockets;
    
    public class HttpResponseSession
    {
        private CancellationToken _token;
        private TcpSession _session;
        
        internal HttpResponseSession(CancellationToken token, TcpSession session)
        {
            _token   = token;
            _session = session;
        }
        
        public async Task Respond(HttpResponse response)
        {
            if (_session.State == TcpSessionState.Closed)
            {
                throw new TcpSessionNotActiveException("Session is no longer active.");
            }
            
            var http = response.ToHttp().ToCharArray();
            await _session.Writer.WriteLineAsync(http, _token);
        }
        
        public void Close()
        {
            _session.Close();
        }
    }
}
