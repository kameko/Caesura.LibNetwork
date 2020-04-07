
namespace Caesura.LibNetwork
{
    using System;
    using System.IO;
    using System.Threading;
    
    public class NetworkSerialization
    {
        public static HttpResponse DeserializeHttpResponse(StreamReader reader, LibNetworkConfig config, CancellationToken token)
        {
            if (reader.EndOfStream)
            {
                throw new EndOfStreamException();
            }
            
            var response_line = reader.ReadLine();
            var message       = DeserializeHttpMessage(reader, config, token);
            var response      = new HttpResponse(response_line!, message);
            
            return response;
        }
        
        public static HttpRequest DeserializeHttpRequest(StreamReader reader, LibNetworkConfig config, CancellationToken token)
        {
            if (reader.EndOfStream)
            {
                throw new EndOfStreamException();
            }
            
            var request_line = reader.ReadLine();
            var message      = DeserializeHttpMessage(reader, config, token);
            var request      = new HttpRequest(request_line!, message);
            
            return request;
        }
        
        public static HttpMessage DeserializeHttpMessage(StreamReader reader, LibNetworkConfig config, CancellationToken token)
        {
            HttpHeaders headers = new HttpHeaders();
            
            var limiter = config.HeaderAmountLimit;
            while (limiter < config.HeaderAmountLimit && !reader.EndOfStream && !token.IsCancellationRequested)
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
            
            var body_lines = reader.EndOfStream ? string.Empty : reader.ReadToEnd();
            var body       = new HttpBody(body_lines);
            var message    = new HttpMessage(headers, body);
            
            return message;
        }
    }
}
