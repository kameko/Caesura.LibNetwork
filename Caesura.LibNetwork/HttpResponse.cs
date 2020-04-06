
namespace Caesura.LibNetwork
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    
    public class HttpResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        public HttpVersion Version { get; set; }
        public HttpMessage Message { get; set; }
        
        public bool IsInformationalStatusCode => CheckStatusCodeInRange(100, 200);
        public bool IsSuccessStatusCode       => CheckStatusCodeInRange(200, 300);
        public bool IsRedirectionStatusCode   => CheckStatusCodeInRange(300, 400);
        public bool IsClientErrorStatusCode   => CheckStatusCodeInRange(400, 500);
        public bool IsServerErrorStatusCode   => CheckStatusCodeInRange(500, 600);
        
        public HttpResponse()
        {
            Message = new HttpMessage();
        }
        
        public HttpResponse(HttpStatusCode code, HttpVersion version, HttpMessage message)
        {
            StatusCode = code;
            Version    = version;
            Message    = message;
        }
        
        public string ToHttp()
        {
            return HttpVersionUtils.HttpVersionToString(Version)
                + " "
                + ((int)StatusCode).ToString()
                + " "
                + StatusCode.ToString()
                + "\r\n"
                + Message.ToHttp();
        }
        
        // TODO: parsing from string
        
        private bool CheckStatusCodeInRange(int begin, int end)
        {
            var code = (int)StatusCode;
            return code >= begin && code < end;
        }
    }
}
