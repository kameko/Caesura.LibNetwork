
namespace Caesura.LibNetwork
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    
    public class HttpBody
    {
        internal string raw_body;
        
        public HttpBody()
        {
            raw_body = string.Empty;
        }
        
        public override string ToString()
        {
            return raw_body;
        }
    }
}
