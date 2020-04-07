
namespace Caesura.LibNetwork
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Text;
    using System.Net.Sockets;
    
    public class NetworkSerialization
    {
        internal static HttpResponse DeserializeHttpResponse(StreamReader reader, LibNetworkConfig config, CancellationToken token)
        {
            if (reader.EndOfStream)
            {
                throw new EndOfStreamException();
            }
            
            var response_line = reader.ReadLine();
            var message  = DeserializeHttpMessage(reader, config, token);
            var response = new HttpResponse(response_line!, message);
            
            return response;
        }
        
        internal static HttpRequest DeserializeHttpRequest(StreamReader reader, LibNetworkConfig config, CancellationToken token)
        {
            if (reader.EndOfStream)
            {
                throw new EndOfStreamException();
            }
            
            var request_line = reader.ReadLine();
            var message = DeserializeHttpMessage(reader, config, token);
            var request = new HttpRequest(request_line!, message);
            
            return request;
        }
        
        internal static HttpMessage DeserializeHttpMessage(StreamReader reader, LibNetworkConfig config, CancellationToken token)
        {
            HttpHeaders headers = new HttpHeaders();
            
            var limiter = config.HeaderAmountLimit;
            while (limiter < config.HeaderAmountLimit && !reader.EndOfStream)
            {
                var header_line = reader.ReadLine();
                if (!string.IsNullOrEmpty(header_line))
                {
                    var header = new HttpHeader(header_line);
                    headers.Add(header);
                }
                else
                {
                    // reached the end of the headers, next is the body.
                    break;
                }
                limiter--;
            }
            
            var body_lines = reader.ReadToEnd();
            var body       = new HttpBody(body_lines);
            var message    = new HttpMessage(headers, body);
            
            return message;
        }
    }
}
