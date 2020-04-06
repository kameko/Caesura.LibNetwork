
namespace Caesura.LibNetwork
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    
    public class HttpRequest
    {
        public HttpRequestKind Kind { get; set; }
        public Uri Resource { get; set; }
        public HttpVersion Version { get; set; }
        public bool IsValid { get; private set; }
        
        public HttpRequest()
        {
            Resource = new Uri("/unknown", UriKind.RelativeOrAbsolute);
        }
        
        public HttpRequest(string line)
        {
            Resource = null!;
            // TODO:
        }
        
        private static HttpRequestKind ParseHttpRequestKind(string request)
        {
            var success = Enum.TryParse<HttpRequestKind>(request, true, out var result);
            return success ? result : HttpRequestKind.Unknown;
        }
    }
}
