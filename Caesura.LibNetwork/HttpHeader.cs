
namespace Caesura.LibNetwork
{
    using System;
    using System.Linq;
    
    public readonly struct HttpHeader
    {
        private readonly string whole_header;
        private readonly string header_name;
        private readonly string header_body;
        private readonly bool is_valid;
        
        public string Header => whole_header;
        public string Name   => header_name;
        public string Body   => header_body;
        public bool IsValid  => is_valid;
        
        public HttpHeader(string header)
        {
            whole_header = header;
            is_valid = TryValidate(header, out header_name, out header_body);
        }
        
        public HttpHeader(string name, string body) : this($"{name}: {body}\r\n")
        {
            
        }
        
        public bool CompareName(string name)
        {
            if (string.IsNullOrEmpty(header_name) || string.IsNullOrWhiteSpace(header_name))
            {
                return false;
            }
            
            return header_name.Equals(name, StringComparison.InvariantCultureIgnoreCase);
        }
        
        public static bool TryValidate(string header, out string name, out string body)
        {
            var result = Validate(header, out name, out body);
            return result == ValidationCode.Valid;
        }
        
        public static void ValidateOrThrow(string header, out string name, out string body)
        {
            var result = Validate(header, out name, out body);
            if (result != ValidationCode.Valid)
            {
                throw new InvalidHttpHeaderException(result);
            }
        }
        
        public static ValidationCode Validate(string header, out string name, out string body)
        {
            if (!header.Contains(':'))
            {
                name = string.Empty;
                body = string.Empty;
                return ValidationCode.NoColon;
            }
            
            var split = SplitHeaderAndSanitize(header);
            name = split.Name;
            body = split.Body;
            
            if (ContainsWhitespace(name))
            {
                return ValidationCode.NameContainsWhitespace;
            }
            
            if (ContainsCRLF(body))
            {
                body = RemoveCRLF(body);
            }
            else
            {
                return ValidationCode.BodyDoesNotEndInCRLF;
            }
            
            return ValidationCode.Valid;
            
            // Local functions.
            
            (string Name, string Body) SplitHeaderAndSanitize(string x)
            {
                var split = header.Split(':', 2);
                var name = split[0];
                var body = split[1].TrimStart();
                return (name, body);
            }
            bool ContainsWhitespace(string x) => x.Any(char.IsWhiteSpace);
            bool ContainsCRLF(string x) => x.EndsWith("\r\n");
            string RemoveCRLF(string x) => x.Remove(x.Length - 2);
        }
        
        public enum ValidationCode
        {
            Unkown                  = 0,
            Valid                   = 1,
            NoColon                 = 2,
            NameContainsWhitespace  = 3,
            BodyDoesNotEndInCRLF    = 4,
        }
    }
}
