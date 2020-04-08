
namespace Caesura.LibNetwork.Http
{
    using System;
    using System.Threading;
    using System.Text;
    using System.IO;
    
    public class HttpMessage : IHttpMessage
    {
        public IHttpHeaders Headers { get; private set; }
        public IHttpBody Body { get; private set; }
        
        public bool IsValid    => Headers.IsValid && Body.IsValid;
        public int HeaderCount => Headers.Count;
        public bool HasHeaders => Headers.HasHeaders;
        public bool HasBody    => Body.HasBody;
        
        public HttpMessage()
        {
            Headers = new HttpHeaders();
            Body    = new HttpBody();
        }
        
        public HttpMessage(IHttpHeaders headers, IHttpBody body)
        {
            Headers    = headers;
            Body       = body;
        }
        
        public HttpMessage(IHttpHeaders headers)
            : this(headers, new HttpBody()) { }
        
        public static HttpMessage FromStream(StreamReader reader, int header_limit, CancellationToken token)
        {
            HttpHeaders headers = new HttpHeaders();
            
            var limiter = header_limit;
            while (limiter < header_limit && !reader.EndOfStream && !token.IsCancellationRequested)
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
        
        public string ToHttp()
        {
            return Headers.ToHttp()
                + "\r\n"
                + Body.ToHttp();
        }
        
        public byte[] ToBytes()
        {
            var http  = ToHttp();
            var bytes = Encoding.UTF8.GetBytes(http);
            return bytes;
        }
        
        public override string ToString()
        {
            return ToHttp();
        }
    }
}
