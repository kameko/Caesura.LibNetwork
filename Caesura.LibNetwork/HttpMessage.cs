
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
        
        public bool IsSuccessStatusCode => CheckIsSuccessStatusCode();
        public bool HasHeaders => Headers.HasHeaders;
        public bool HasBody => Body.HasBody;
        public int HeaderCount => Headers.Count;
        
        public HttpMessage()
        {
            Headers = new HttpHeaders();
            Body    = new HttpBody();
        }
        
        private bool CheckIsSuccessStatusCode()
        {
            var code = (int)StatusCode;
            // an HTTP status code is defined as being a
            // success code if it is in the range of 2XX
            return code >= 200 && code < 300;
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
