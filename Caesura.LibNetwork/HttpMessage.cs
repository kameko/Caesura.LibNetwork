
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
        public HttpStatusCode StatusCode { get; set; }
        public HttpRequest Request { get; set; }
        public HttpHeaders Headers { get; set; }
        public HttpBody Body { get; set; }
        
        public bool IsInformationalStatusCode => CheckStatusCodeInRange(100, 200);
        public bool IsSuccessStatusCode       => CheckStatusCodeInRange(200, 300);
        public bool IsRedirectionStatusCode   => CheckStatusCodeInRange(300, 400);
        public bool IsClientErrorStatusCode   => CheckStatusCodeInRange(400, 500);
        public bool IsServerErrorStatusCode   => CheckStatusCodeInRange(500, 600);
        
        public int HeaderCount => Headers.Count;
        public bool HasHeaders => Headers.HasHeaders;
        public bool HasBody    => Body.HasBody;
        
        public HttpMessage()
        {
            Headers = new HttpHeaders();
            Body    = new HttpBody();
        }
        
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
    }
}
