
namespace Caesura.LibNetwork.Http
{
    using System;
    using System.Threading;
    using System.IO;
    
    public class HttpConfig
    {
        public HttpConfigFactories Factories { get; set; }
        public int HeaderAmountLimit { get; set; }
        public int ConnectionLoopMillisecondDelayInterval { get; set; }
        public bool ThreadPerConnection { get; set; }
        public TimeSpan SessionTimeout { get; set; }
        
        public HttpConfig()
        {
            Factories                              = HttpConfigFactories.GetDefault();
            HeaderAmountLimit                      = 100;
            ConnectionLoopMillisecondDelayInterval = -1;
            ThreadPerConnection                    = true;
            SessionTimeout                         = TimeSpan.FromMinutes(1);
        }
        
        public static HttpConfig GetDefault()
        {
            return new HttpConfig();
        }
    }
    
    public class HttpConfigFactories
    {
        public Func<LibNetworkConfig, StreamReader, CancellationToken, IHttpRequest> HttpRequestFactory { get; set; }
        public Func<LibNetworkConfig, ITcpSession, CancellationToken, IHttpSession> HttpSessionFactory { get; set; }
        
        public HttpConfigFactories()
        {
            HttpRequestFactory = (c, s, t) => HttpRequest.FromStream(s, c.Http.HeaderAmountLimit, t);
            HttpSessionFactory = (c, p, t) => new HttpSession(c, p, t);
        }
        
        public static HttpConfigFactories GetDefault()
        {
            return new HttpConfigFactories();
        }
    }
}
