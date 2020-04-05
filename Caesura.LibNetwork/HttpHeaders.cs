
namespace Caesura.LibNetwork
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    
    public class HttpHeaders : IEnumerable<HttpHeader>
    {
        private List<HttpHeader> Headers;
        
        public HttpHeaders()
        {
            Headers = new List<HttpHeader>();
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
