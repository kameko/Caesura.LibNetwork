
namespace Caesura.LibNetwork
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    
    public class HttpHeaders : IEnumerable<HttpHeader>
    {
        private bool is_valid_set;
        private bool is_valid;
        private List<HttpHeader> Headers;
        
        public bool IsValid => CheckIsValid();
        
        public HttpHeaders()
        {
            Headers = new List<HttpHeader>();
        }
        
        public HttpHeaders(IEnumerable<HttpHeader> headers)
        {
            Headers = new List<HttpHeader>(headers);
        }
        
        public void Add(HttpHeader header)
        {
            Headers.Add(header);
            is_valid_set = false;
        }
        
        public IEnumerable<HttpHeader> GetAllValid()
        {
            return Headers.Where(x => x.IsValid);
        }
        
        public IEnumerable<HttpHeader> GetAllInvalid()
        {
            return Headers.Where(x => !x.IsValid);
        }
        
        public HttpHeader GetByName(string name)
        {
            return Headers.Find(x => x.CompareName(name));
        }
        
        public IEnumerable<HttpHeader> GetAll()
        {
            return new List<HttpHeader>(Headers);
        }
        
        private bool CheckIsValid()
        {
            if (is_valid_set)
            {
                return is_valid;
            }
            
            is_valid     = Headers.Any(x => x.IsValid);
            is_valid_set = true;
            return is_valid;
        }
        
        public HttpHeader this[int index]  
        {  
            get { return Headers[index]; }  
            set { Headers.Insert(index, value); }  
        } 

        public IEnumerator<HttpHeader> GetEnumerator()
        {
            return Headers.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
