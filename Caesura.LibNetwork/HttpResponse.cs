
namespace Caesura.LibNetwork
{
    using System;
    
    public class HttpResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        public HttpVersion Version { get; set; }
        public HttpMessage Message { get; set; }
        
        public bool IsInformationalStatusCode => HttpStatusCodeUtils.CheckStatusCodeInRange(StatusCode, 100, 200);
        public bool IsSuccessStatusCode       => HttpStatusCodeUtils.CheckStatusCodeInRange(StatusCode, 200, 300);
        public bool IsRedirectionStatusCode   => HttpStatusCodeUtils.CheckStatusCodeInRange(StatusCode, 300, 400);
        public bool IsClientErrorStatusCode   => HttpStatusCodeUtils.CheckStatusCodeInRange(StatusCode, 400, 500);
        public bool IsServerErrorStatusCode   => HttpStatusCodeUtils.CheckStatusCodeInRange(StatusCode, 500, 600);
        
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
        
    }
}
