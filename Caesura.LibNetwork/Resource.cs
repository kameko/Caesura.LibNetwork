
namespace Caesura.LibNetwork
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    
    public class Resource
    {
        private string _resource;
        
        public string Representation => IsValid ? _resource : throw new InvalidOperationException("Resource is not valid.");
        public bool IsValid { get; private set; }
        
        public bool IsIndex         => _resource == "/";
        public bool IsDirectory     => _resource.Length > 1 && _resource.EndsWith('/');
        public bool IsFile          => !_resource.EndsWith('/');
        public bool IsWellFormedUri => Uri.IsWellFormedUriString(_resource, UriKind.Relative);
        
        public Resource()
        {
            _resource = string.Empty;
            IsValid   = false;
        }
        
        public Resource(string item)
        {
            IsValid = TryValidate(item, out _resource);
        }
        
        public static bool TryValidate(string resource, out string result)
        {
            var r = Validate(resource, out result);
            return r == ValidationCode.Valid;
        }
        
        public static void ValidateOrThrow(string resource, out string result)
        {
            var r = Validate(resource, out result);
            if (r != ValidationCode.Valid)
            {
                throw new UriFormatException(r.ToString());
            }
        }
        
        public static ValidationCode Validate(string resource, out string result)
        {
            result = string.Empty;
            
            if (string.IsNullOrEmpty(resource))
            {
                return ValidationCode.ResourceIsEmpty;
            }
            if (!resource.StartsWith('/'))
            {
                return ValidationCode.DoesNotStartWithForwardslash;
            }
            
            var lastchar = '\0';
            foreach (var c in resource)
            {
                if (c == '/' && lastchar == '/')
                {
                    return ValidationCode.ContainsRepeatingForwardslashes;
                }
                if (char.IsWhiteSpace(c))
                {
                    return ValidationCode.ContainsSpaces;
                }
                
                lastchar = c;
            }
            
            result = resource;
            return ValidationCode.Valid;
        }
        
        public override string ToString()
        {
            return _resource;
        }
        
        public enum ValidationCode
        {
            Unknown                         = 0,
            Valid                           = 1,
            ResourceIsEmpty                 = 2,
            DoesNotStartWithForwardslash    = 3,
            ContainsRepeatingForwardslashes = 4,
            ContainsInvalidCharacters       = 5,
            ContainsSpaces                  = 6,
        }
    }
}
