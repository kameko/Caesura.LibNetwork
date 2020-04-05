
namespace Caesura.LibNetwork
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Net;
    using System.Net.Http;
    
    public class HttpResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        // TODO: getter that decides for itself if StatusCode is a success
        public bool IsSuccessStatusCode { get; set; }
        public HttpRequest Request { get; set; }
        public HttpHeaders Headers { get; set; }
        public HttpBody? Body { get; set; }
        
        public HttpResponse()
        {
            Headers = new HttpHeaders();
        }
    }
    
    public class HttpResponse<T> : HttpResponse
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
        
        public HttpResponse() : base()
        {
            is_entity_set = false;
            _entity       = default!;
        }
        
        public HttpResponse(T entity) : this()
        {
            Entity = entity;
        }
    }
}
