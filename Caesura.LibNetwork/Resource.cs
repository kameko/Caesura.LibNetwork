
namespace Caesura.LibNetwork
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    
    public class Resource
    {
        private string _resource;
        
        public bool IsValid { get; private set; }
        
        public Resource()
        {
            _resource = null!;
            IsValid  = false;
        }
        
        public Resource(string item)
        {
            _resource = item;
            IsValid   = !string.IsNullOrEmpty(item);
        }
        
        public override string ToString()
        {
            return _resource;
        }
    }
}
