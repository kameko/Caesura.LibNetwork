
namespace Caesura.LibNetwork.Http
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    
    // TODO: maybe turn this into an HttpSession class,
    // move all the response events here.
    
    public class HttpResponseSession
    {
        private ITcpSession _session;
        private CancellationToken _token;
        
        internal HttpResponseSession(ITcpSession session, CancellationToken token)
        {
            _session = session;
            _token   = token;
        }
        
        public async Task Respond(IHttpResponse response)
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
