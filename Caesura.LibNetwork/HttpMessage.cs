
namespace Caesura.LibNetwork
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Net;
    using System.Net.Http;
    
    public class HttpMessage
    {
        public HttpStatusCode StatusCode { get; private set; }
        public HttpRequest Request { get; private set; }
        public HttpHeaders Headers { get; private set; }
        public HttpBody Body { get; private set; }
        
        public bool IsInformationalStatusCode => CheckStatusCodeInRange(100, 200);
        public bool IsSuccessStatusCode       => CheckStatusCodeInRange(200, 300);
        public bool IsRedirectionStatusCode   => CheckStatusCodeInRange(300, 400);
        public bool IsClientErrorStatusCode   => CheckStatusCodeInRange(400, 500);
        public bool IsServerErrorStatusCode   => CheckStatusCodeInRange(500, 600);
        
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
        
        public HttpMessage(HttpStatusCode status, HttpRequest request, HttpHeaders headers, HttpBody body)
        {
            StatusCode = status;
            Request    = request;
            Headers    = headers;
            Body       = body;
        }
        
        public HttpMessage(HttpRequest request, HttpHeaders headers, HttpBody body)
            : this(HttpStatusCode.OK, request, headers, body) { }
            
        public HttpMessage(HttpStatusCode status, HttpRequest request, HttpHeaders headers)
            : this(status, request, headers, new HttpBody()) { }
            
        public HttpMessage(HttpRequest request, HttpHeaders headers)
            : this(HttpStatusCode.OK, request, headers, new HttpBody()) { }
        
        private bool CheckStatusCodeInRange(int begin, int end)
        {
            var code = (int)StatusCode;
            return code >= begin && code < end;
        }
    }
    
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
        
        public HttpMessage(HttpStatusCode status, HttpRequest request, HttpHeaders headers, HttpBody body, T entity)
            : base(status, request, headers, body)
        {
            _entity = default!;
            Entity  = entity;
        }
        
        public HttpMessage(HttpRequest request, HttpHeaders headers, HttpBody body, T entity)
            : this(HttpStatusCode.OK, request, headers, body, entity) { }
            
        public HttpMessage(HttpStatusCode status, HttpRequest request, HttpHeaders headers, T entity)
            : this(status, request, headers, new HttpBody(), entity) { }
            
        public HttpMessage(HttpRequest request, HttpHeaders headers, T entity)
            : this(HttpStatusCode.OK, request, headers, new HttpBody(), entity) { }
    }
}
