
namespace Caesura.Option
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    
    public readonly struct Unit
    {
        public override bool Equals(object? obj)
        {
            return obj is Unit;
        }
        
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        
        public override string ToString()
        {
            return "<None>";
        }
    }
}
