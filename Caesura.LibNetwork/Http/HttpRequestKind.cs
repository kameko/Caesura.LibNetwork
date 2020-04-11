
namespace Caesura.LibNetwork.Http
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
        HEAD    = 6,
        PATCH   = 7,
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
