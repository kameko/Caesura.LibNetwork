
namespace Caesura.LibNetwork
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Net;
    using System.Net.Http;
    
    public class NetworkResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        public bool IsSuccessStatusCode { get; set; }
        
        public NetworkResponse()
        {
            
        }
    }
    
    public class NetworkResponse<T> : NetworkResponse
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
        
        public NetworkResponse() : base()
        {
            is_entity_set = false;
            _entity       = default!;
        }
        
        public NetworkResponse(T entity) : this()
        {
            Entity = entity;
        }
        
        
    }
}
