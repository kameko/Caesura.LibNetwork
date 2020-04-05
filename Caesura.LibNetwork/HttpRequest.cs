
namespace Caesura.LibNetwork
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    
    public enum HttpRequest
    {
        None    = 0,
        GET     = 1,
        DELETE  = 2,
        PUT     = 3,
        POST    = 4,
        PATCH   = 5,
        HEAD    = 6,
        TRACE   = 7,
        OPTIONS = 8,
        CONNECT = 9,
    }
}
