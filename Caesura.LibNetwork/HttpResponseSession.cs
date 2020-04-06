
namespace Caesura.LibNetwork
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Net.Sockets;
    
    public class HttpResponseSession
    {
        private CancellationToken _token;
        private NetworkStream _stream;
        
        public HttpResponseSession(CancellationToken token, NetworkStream stream)
        {
            _token  = token;
            _stream = stream;
        }
        
        public async Task Respond(HttpResponse response)
        {
            var bytes = response.ToBytes();
            await _stream.WriteAsync(bytes, _token);
        }
    }
}
