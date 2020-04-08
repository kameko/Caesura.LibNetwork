
namespace Caesura.LibNetwork.Http
{
    using System;
    using System.Threading;
    using System.Text;
    using System.IO;
    
    public class HttpResponse : IHttpResponse
    {
        public HttpVersion Version { get; private set; }
        public HttpStatusCode StatusCode { get; private set; }
        public IHttpMessage Message { get; private set; }
        
        public bool IsInformationalStatusCode => HttpStatusCodeUtils.CheckStatusCodeInRange(StatusCode, 100, 200);
        public bool IsSuccessStatusCode       => HttpStatusCodeUtils.CheckStatusCodeInRange(StatusCode, 200, 300);
        public bool IsRedirectionStatusCode   => HttpStatusCodeUtils.CheckStatusCodeInRange(StatusCode, 300, 400);
        public bool IsClientErrorStatusCode   => HttpStatusCodeUtils.CheckStatusCodeInRange(StatusCode, 400, 500);
        public bool IsServerErrorStatusCode   => HttpStatusCodeUtils.CheckStatusCodeInRange(StatusCode, 500, 600);
        
        private bool is_valid;
        public bool IsValid => is_valid && Message.IsValid;
        
        private HttpResponse()
        {
            Message = new HttpMessage();
        }
        
        public HttpResponse(string line, IHttpMessage message)
        {
            is_valid   = TryValidate(line, out var version, out var status);
            Version    = version;
            StatusCode = status;
            Message    = message;
        }
        
        public HttpResponse(HttpVersion version, HttpStatusCode code, IHttpMessage message)
        {
            Version    = version;
            StatusCode = code;
            Message    = message;
        }
        
        public static HttpResponse FromStream(StreamReader reader, int header_limit, CancellationToken token)
        {
            if (reader.EndOfStream)
            {
                throw new EndOfStreamException();
            }
            
            var response_line = reader.ReadLine();
            var message       = HttpMessage.FromStream(reader, header_limit, token);
            var response      = new HttpResponse(response_line!, message);
            
            return response;
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
        
        public byte[] ToBytes()
        {
            var http  = ToHttp();
            var bytes = Encoding.UTF8.GetBytes(http);
            return bytes;
        }
        
        public static bool TryValidate(string response, out HttpVersion version, out HttpStatusCode status)
        {
            var result = Validate(response, out version, out status);
            return result == ValidationCode.Valid;
        }
        
        public static void ValidateOrThrow(string response, out HttpVersion version, out HttpStatusCode status)
        {
            var result = Validate(response, out version, out status);
            if (result != ValidationCode.Valid)
            {
                throw new InvalidHttpResponseException(result);
            }
        }
        
        public static ValidationCode Validate(string response, out HttpVersion version, out HttpStatusCode status)
        {
            var elements = response?.Split(' ') ?? new string[0];
            
            if (elements.Length > 0)
            {
                version = HttpVersionUtils.Parse(elements[0]);
                if (version == HttpVersion.Unknown)
                {
                    status = HttpStatusCode.Unkown;
                    return ValidationCode.UnknownVersion;
                }
            }
            else
            {
                version = HttpVersion.Unknown;
                status  = HttpStatusCode.Unkown;
                return ValidationCode.NoVersion;
            }
            
            if (elements.Length > 1)
            {
                var stat_int_success = int.TryParse(elements[1], out int stat_int);
                if (!stat_int_success)
                {
                    status = HttpStatusCode.Unkown;
                    return ValidationCode.StatusNotInt;
                }
                
                var stat_sucecss = HttpStatusCodeUtils.ConvertFromNumber(stat_int, out status);
                if (!stat_sucecss)
                {
                    return ValidationCode.UnknownStatus;
                }
            }
            else
            {
                status = HttpStatusCode.Unkown;
                return ValidationCode.NoStatus;
            }
            
            return ValidationCode.Valid;
        }
        
        public override string ToString()
        {
            return ToHttp();
        }
        
        public enum ValidationCode
        {
            Unknown        = 0,
            Valid          = 1,
            UnknownVersion = 2,
            NoVersion      = 3,
            NoStatus       = 4,
            StatusNotInt   = 5,
            UnknownStatus  = 6,
        }
    }
}
