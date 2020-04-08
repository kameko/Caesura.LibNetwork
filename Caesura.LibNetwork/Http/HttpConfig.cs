
namespace Caesura.LibNetwork.Http
{
    using System;
    using System.Threading;
    using System.IO;
    
    public class HttpConfig
    {
        public HttpConfigFactories Factories { get; set; }
        public int HeaderAmountLimit { get; set; }
        
        public HttpConfig()
        {
            Factories         = HttpConfigFactories.GetDefault();
            HeaderAmountLimit = 100;
        }
        
        public static HttpConfig GetDefault()
        {
            return new HttpConfig();
        }
    }
    
    public class HttpConfigFactories
    {
        public Func<LibNetworkConfig, StreamReader, CancellationToken, IHttpRequest> HttpRequestFactory { get; set; }
        
        public HttpConfigFactories()
        {
            HttpRequestFactory = (c, s, t) => HttpRequest.FromStream(s, c.Http.HeaderAmountLimit, t);
        }
        
        public static HttpConfigFactories GetDefault()
        {
            return new HttpConfigFactories();
        }
    }
}
