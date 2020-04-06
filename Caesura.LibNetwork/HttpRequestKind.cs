
namespace Caesura.LibNetwork
{
    using System;
    
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
    
    public static class HttpRequestKindUtils
    {
        public static string ConvertToString(HttpRequestKind kind)
        {
            return kind.ToString();
        }
        
        public static HttpRequestKind ParseHttpRequestKind(string request)
        {
            var success = Enum.TryParse<HttpRequestKind>(request, true, out var result);
            return success ? result : HttpRequestKind.Unknown;
        }
    }
}
