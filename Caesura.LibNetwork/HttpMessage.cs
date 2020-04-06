
namespace Caesura.LibNetwork
{
    using System;
    using System.Text;
    
    public class HttpMessage
    {
        public HttpHeaders Headers { get; private set; }
        public HttpBody Body { get; private set; }
        
        public bool IsValid    => Headers.IsValid && Body.IsValid;
        public int HeaderCount => Headers.Count;
        public bool HasHeaders => Headers.HasHeaders;
        public bool HasBody    => Body.HasBody;
        
        public HttpMessage()
        {
            Headers = new HttpHeaders();
            Body    = new HttpBody();
        }
        
        public HttpMessage(HttpHeaders headers, HttpBody body)
        {
            Headers    = headers;
            Body       = body;
        }
        
        public HttpMessage(HttpHeaders headers)
            : this(headers, new HttpBody()) { }
        
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
