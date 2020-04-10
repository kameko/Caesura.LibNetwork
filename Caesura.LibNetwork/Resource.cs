
namespace Caesura.LibNetwork
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    
    public class Resource
    {
        private string _resource;
        
        public string Representation => _resource;
        public bool IsValid { get; private set; }
        
        public Resource()
        {
            _resource = null!;
            IsValid   = false;
        }
        
        public Resource(string item)
        {
            IsValid = TryValidate(item, out _resource);
        }
        
        public bool IsWellFormedUri() => IsWellFormedUri(UriKind.RelativeOrAbsolute);
        public bool IsWellFormedUri(UriKind kind) => Uri.IsWellFormedUriString(_resource, kind);
        
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
