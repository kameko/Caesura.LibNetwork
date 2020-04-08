
namespace Caesura.LibNetwork
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    
    public class HttpResponseSession
    {
        private ITcpSession _session;
        private CancellationToken _token;
        
        internal HttpResponseSession(ITcpSession session, CancellationToken token)
        {
            _session = session;
            _token   = token;
        }
        
        public async Task Respond(HttpResponse response)
        {
            if (!response.IsValid)
            {
                throw new ArgumentException("HttpResponse is not valid to send.");
            }
            if (_session.State == TcpSessionState.Closed)
            {
                throw new TcpSessionNotActiveException("Session is no longer active.");
            }
            
            var http = response.ToHttp();
            await _session.Write(http, _token);
        }
        
        public void Close()
        {
            _session.Close();
        }
    }
}
