
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
                if (string.IsNullOrEmpty(header_line))
                {
                    // reached the end of the headers, next is the body.
                    break;
                }
                else
                {
                    var header = new HttpHeader(header_line);
                    headers.Add(header);
                }
            }
            
            var body_lines = reader.ReadToEnd();
            var body       = new HttpBody(body_lines);
            
            var message    = new HttpMessage(headers, body);
            
            return message;
        }
        
        // Old methods
        
        internal static HttpResponse GetResponse(CancellationToken token, LibNetworkConfig config, NetworkStream stream)
        {
            var sb           = new StringBuilder();
            var response_str = ReadLine(token, stream, sb, config.HeaderCharReadLimit);
            var message      = GetMessage(token, config, stream, sb);
            var response     = new HttpResponse(response_str, message);
            return response;
        }
        
        internal static HttpRequest GetRequest(CancellationToken token, LibNetworkConfig config, NetworkStream stream)
        {
            var sb          = new StringBuilder();
            var request_str = ReadLine(token, stream, sb, config.HeaderCharReadLimit);
            var message     = GetMessage(token, config, stream, sb);
            var request     = new HttpRequest(request_str, message);
            return request;
        }
        
        internal static HttpMessage GetMessage(CancellationToken token, LibNetworkConfig config, NetworkStream stream, StringBuilder sb)
        {
            var headers = GetHeaders(token, stream, sb, config.HeaderAmountLimit, config.HeaderCharReadLimit);
            var body    = GetBody(token, stream, sb, config.BodyCharReadLimit);
            var message = new HttpMessage(headers, body);
            return message;
        }
        
        internal static HttpHeaders GetHeaders(CancellationToken token, NetworkStream stream, StringBuilder sb, int header_limit, int header_char_limit)
        {
            var headers = new HttpHeaders();
            var line    = string.Empty;
            while (headers.Count <= header_limit)
            {
                line = ReadLine(token, stream, sb, header_char_limit);
                
                // If all we get back is a newline, that means we're done
                // with the headers. Next we read the body.
                if (line == "\r\n")
                {
                    break;
                }
                if (string.IsNullOrEmpty(line))
                {
                    break;
                }
                
                var header = new HttpHeader(line);
                headers.Add(header);
            }
            
            return headers;
        }
        
        internal static HttpBody GetBody(CancellationToken token, NetworkStream stream, StringBuilder sb, int body_limit)
        {
            sb.Clear();
            
            char current_char = '\0';
            int current_int   = 0;
            while (current_int > -1 && current_int <= body_limit && !token.IsCancellationRequested)
            {
                current_int  = stream.ReadByte();
                current_char = Convert.ToChar(current_int);
                sb.Append(current_char);
            }
            
            var body = new HttpBody(sb.ToString());
            return body;
        }
        
        internal static string ReadLine(CancellationToken token, NetworkStream stream, StringBuilder sb, int limit)
        {
            sb.Clear();
            
            char last_char    = '\0';
            char current_char = '\0';
            int current_int   = 0;
            while (current_int > -1 && current_int <= limit && !token.IsCancellationRequested)
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
    }
}
