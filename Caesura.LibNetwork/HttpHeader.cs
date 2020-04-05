
namespace Caesura.LibNetwork
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    
    public readonly struct HttpHeader
    {
        private readonly string whole_header;
        private readonly string header_name;
        private readonly string header_body;
        private readonly bool is_valid;
        
        public string Header => whole_header;
        public string Name => header_name;
        public string Body => header_body;
        public bool IsValid => is_valid;
        
        public HttpHeader(string header)
        {
            whole_header = header;
            is_valid = Validate(header, out header_name, out header_body);
        }
        
        public HttpHeader(string name, string body) : this($"{name}: {body}\r\n")
        {
            
        }
        
        public bool CompareName(string name)
        {
            if (string.IsNullOrEmpty(header_name))
            {
                return false;
            }
            
            return header_name.Equals(name, StringComparison.InvariantCultureIgnoreCase);
        }
        
        public static bool Validate(string header, out string name, out string body)
        {
            if (!header.Contains(':'))
            {
                name = string.Empty;
                body = string.Empty;
                return false;
            }
            
            var split = header.Split(':', 2);
            name = split[0];
            body = split[1];
            
            if (name.Any(Char.IsWhiteSpace))
            {
                return false; // invalid, name can only have dashes.
            }
            if (!header.EndsWith("\r\n"))
            {
                return false; // header must end in <CR><LF>
            }
            
            return true;
        }
    }
}
