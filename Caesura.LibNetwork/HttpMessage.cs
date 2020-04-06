
namespace Caesura.LibNetwork
{
    using System;
    
    public class HttpMessage
    {
        public HttpRequest Request { get; private set; }
        public HttpHeaders Headers { get; private set; }
        public HttpBody Body { get; private set; }
        
        public bool IsValid    => Request.IsValid && Headers.IsValid && Body.IsValid;
        public int HeaderCount => Headers.Count;
        public bool HasHeaders => Headers.HasHeaders;
        public bool HasBody    => Body.HasBody;
        
        public HttpMessage()
        {
            Request = new HttpRequest();
            Headers = new HttpHeaders();
            Body    = new HttpBody();
        }
        
        public HttpMessage(HttpRequest request, HttpHeaders headers, HttpBody body)
        {
            Request    = request;
            Headers    = headers;
            Body       = body;
        }
        
        public HttpMessage(HttpRequest request, HttpHeaders headers)
            : this(request, headers, new HttpBody()) { }
        
        public string ToHttp()
        {
            throw new NotImplementedException();
        }
        
        public override string ToString()
        {
            return ToHttp();
        }
    }
}
