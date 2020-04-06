
namespace Caesura.LibNetwork
{
    using System;
    
    public class HttpResponse
    {
        public HttpStatusCode StatusCode { get; private set; }
        public HttpVersion Version { get; private set; }
        public HttpMessage Message { get; private set; }
        
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
            return HttpVersionUtils.ConvertToString(Version)
                + " "
                + HttpStatusCodeUtils.ConvertToNumberString(StatusCode)
                + " "
                + HttpStatusCodeUtils.ConvertToFormattedString(StatusCode)
                + "\r\n"
                + Message.ToHttp();
        }
        
        // TODO: parsing from string
        
    }
}
