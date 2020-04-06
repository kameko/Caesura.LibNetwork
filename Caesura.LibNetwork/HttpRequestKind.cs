
namespace Caesura.LibNetwork
{
    public enum HttpRequestKind
    {
        None    = 0,
        Unknown = 1,
        GET     = 2,
        DELETE  = 3,
        PUT     = 4,
        POST    = 5,
        PATCH   = 6,
        HEAD    = 7,
        TRACE   = 8,
        OPTIONS = 9,
        CONNECT = 10,
    }
}
