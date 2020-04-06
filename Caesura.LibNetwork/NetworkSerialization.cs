
namespace Caesura.LibNetwork
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Text;
    using System.Net.Sockets;
    
    public class NetworkSerialization
    {
        internal static HttpResponse GetResponse(LibNetworkConfig config, NetworkStream stream)
        {
            var sb           = new StringBuilder();
            var response_str = ReadLine(stream, sb, config.HeaderCharReadLimit);
            var message      = GetMessage(config, stream, sb);
            var response     = new HttpResponse(response_str, message);
            return response;
        }
        
        internal static HttpRequest GetRequest(LibNetworkConfig config, NetworkStream stream)
        {
            var sb          = new StringBuilder();
            var request_str = ReadLine(stream, sb, config.HeaderCharReadLimit);
            var message     = GetMessage(config, stream, sb);
            var request     = new HttpRequest(request_str, message);
            return request;
        }
        
        internal static HttpMessage GetMessage(LibNetworkConfig config, NetworkStream stream, StringBuilder sb)
        {
            var headers = GetHeaders(stream, sb, config.HeaderAmountLimit, config.HeaderCharReadLimit);
            var body    = GetBody(stream, sb, config.BodyCharReadLimit);
            var message = new HttpMessage(headers, body);
            return message;
        }
        
        internal static HttpHeaders GetHeaders(NetworkStream stream, StringBuilder sb, int header_limit, int header_char_limit)
        {
            var headers = new HttpHeaders();
            var line    = string.Empty;
            while (headers.Count <= header_limit)
            {
                line = ReadLine(stream, sb, header_char_limit);
                
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
        
        internal static HttpBody GetBody(NetworkStream stream, StringBuilder sb, int body_limit)
        {
            sb.Clear();
            
            char current_char = '\0';
            int current_int   = 0;
            while (current_int > -1 && current_int <= body_limit)
            {
                current_int  = stream.ReadByte();
                current_char = Convert.ToChar(current_int);
                sb.Append(current_char);
            }
            
            var body = new HttpBody(sb.ToString());
            return body;
        }
        
        internal static string ReadLine(NetworkStream stream, StringBuilder sb, int limit)
        {
            sb.Clear();
            
            char last_char    = '\0';
            char current_char = '\0';
            int current_int   = 0;
            while (current_int > -1 && current_int <= limit)
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
