
namespace Caesura.LibNetwork.Http
{
    public enum HttpVersion
    {
        Unknown = 0,
        HTTP0_9 = 1,
        HTTP1_0 = 2,
        HTTP1_1 = 3,
        HTTP2   = 4,
        HTTP3   = 5,
    }
    
    public static class HttpVersionUtils
    {
        public static HttpVersion Parse(string version)
        {
            return version.ToUpper() switch
            {
                "HTTP/0.9" => HttpVersion.HTTP0_9,
                "HTTP/1"   => HttpVersion.HTTP1_0,
                "HTTP/1.0" => HttpVersion.HTTP1_0,
                "HTTP/1.1" => HttpVersion.HTTP1_1,
                "HTTP/2"   => HttpVersion.HTTP2,
                "HTTP/2.0" => HttpVersion.HTTP2,
                "HTTP/3"   => HttpVersion.HTTP3,
                "HTTP/3.0" => HttpVersion.HTTP3,
                _          => HttpVersion.Unknown
            };
        }
        
        public static string ConvertToString(HttpVersion version)
        {
            return "HTTP/" + (version switch
            {
                HttpVersion.HTTP0_9 => "0.9",
                HttpVersion.HTTP1_0 => "1.0",
                HttpVersion.HTTP1_1 => "1.1",
                HttpVersion.HTTP2   => "2",
                HttpVersion.HTTP3   => "3",
                _                   => "Unknown",
            });
        }
    }
}
