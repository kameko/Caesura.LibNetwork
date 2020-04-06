
namespace Caesura.LibNetwork
{
    using System;
    
    // TODO: need a way to construct a response message
    //  - a status line which includes the status code and reason message 
    //    (e.g., HTTP/1.1 200 OK, which indicates that the client's request succeeded)
    //  - response header fields (e.g., Content-Type: text/html)
    //  - an empty line
    //  - an optional message body
    
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
        
        public string ToHttp() => ToString();
        
        public override string ToString()
        {
            throw new NotImplementedException();
        }
    }
    
    // TODO: Deprecate for HttpBody.Deserialize<T>()
    public class HttpMessage<T> : HttpMessage
    {
        private bool is_entity_set;
        private T _entity;
        
        public T Entity
        {
            get => is_entity_set ? _entity : throw new InvalidOperationException("Entity is not set.");
            set
            {
                _entity       = value;
                is_entity_set = !(value is null);
            }
        }
        public bool HasEntity => is_entity_set;
        
        public HttpMessage() : base()
        {
            is_entity_set = false;
            _entity       = default!;
        }
        
        public HttpMessage(T entity) : this()
        {
            Entity = entity;
        }
        
        public HttpMessage(HttpRequest request, HttpHeaders headers, HttpBody body, T entity)
            : base(request, headers, body)
        {
            _entity = default!;
            Entity  = entity;
        }
        
        public HttpMessage(HttpRequest request, HttpHeaders headers, T entity)
            : this(request, headers, new HttpBody(), entity) { }
    }
}
