
namespace Caesura.LibNetwork
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Text.Json;
    
    public class HttpBody
    {
        internal string raw_body;
        
        public string Body => raw_body;
        public bool HasBody => !(string.IsNullOrEmpty(raw_body) || string.IsNullOrWhiteSpace(raw_body));
        public bool IsValid => true;
        
        public HttpBody()
        {
            raw_body = string.Empty;
        }
        
        public HttpBody(string body)
        {
            raw_body = Sanitize(body);
        }
        
        public HttpBodyDeserializationResult Deserialize<T>(out T item)
        {
            throw new NotImplementedException();
        }
        
        private string Sanitize(string body)
        {
            if (body.StartsWith("\r\n"))
            {
                body = body.Substring(2);
            }
            return body;
        }
        
        public override string ToString()
        {
            return raw_body;
        }
        
        public enum HttpBodyDeserializationResult
        {
            Unknown = 0,
            Ok      = 1,
        }
    }
}
