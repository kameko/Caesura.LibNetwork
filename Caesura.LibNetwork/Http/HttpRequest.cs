
namespace Caesura.LibNetwork.Http
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Text;
    using System.IO;
    
    public class HttpRequest : IHttpRequest
    {
        public HttpRequestKind Kind { get; private set; }
        public Uri Resource { get; private set; }
        public HttpVersion Version { get; private set; }
        public IHttpMessage Message { get; set; }
        
        private bool is_valid;
        public bool IsValid => is_valid && Message.IsValid;
        
        private HttpRequest()
        {
            Resource = new Uri("/unknown", UriKind.RelativeOrAbsolute);
            Message  = new HttpMessage();
        }
        
        public HttpRequest(string line, IHttpMessage message)
        {
            is_valid = TryValidate(line, out var kind, out var resource, out var version);
            Kind     = kind;
            Resource = resource;
            Version  = version;
            Message  = message;
        }
        
        public HttpRequest(HttpRequestKind kind, Uri resource, HttpVersion version, IHttpMessage message)
        {
            Kind     = kind;
            Resource = resource;
            Version  = version;
            Message  = message;
        }
        
        public HttpRequest(HttpRequestKind kind, string resource, HttpVersion version, IHttpMessage message)
        {
            Kind     = kind;
            Resource = new Uri(resource, UriKind.RelativeOrAbsolute);
            Version  = version;
            Message  = message;
        }
        
        public static HttpRequest FromStream(StreamReader reader, int header_limit, CancellationToken token)
        {
            if (reader.EndOfStream)
            {
                throw new EndOfStreamException();
            }
            
            var request_line = reader.ReadLine();
            var message      = HttpMessage.FromStream(reader, header_limit, token);
            var request      = new HttpRequest(request_line!, message);
            
            return request;
        }
        
        public string ToHttp()
        {
            return HttpRequestKindUtils.ConvertToString(Kind)
                + " "
                + Resource.ToString()
                + " "
                + HttpVersionUtils.ConvertToString(Version)
                + "\r\n"
                + Message.ToHttp();
        }
        
        public byte[] ToBytes()
        {
            var http  = ToHttp();
            var bytes = Encoding.UTF8.GetBytes(http);
            return bytes;
        }
        
        public static bool TryValidate(string request, out HttpRequestKind kind, out Uri resource, out HttpVersion version)
        {
            var result = Validate(request, out kind, out resource, out version);
            return result == ValidationCode.Valid;
        }
        
        public static void ValidateOrThrow(string request, out HttpRequestKind kind, out Uri resource, out HttpVersion version)
        {
            var result = Validate(request, out kind, out resource, out version);
            if (result != ValidationCode.Valid)
            {
                throw new InvalidHttpRequestException(result);
            }
        }
        
        public static ValidationCode Validate(string request, out HttpRequestKind kind, out Uri resource, out HttpVersion version)
        {
            var elements = request?.Split(' ') ?? new string[0];
            
            kind = elements.Length > 0 ? HttpRequestKindUtils.ParseHttpRequestKind(elements[0]) : HttpRequestKind.Unknown;
            if (kind == HttpRequestKind.Unknown)
            {
                resource = new Uri("/unknown", UriKind.RelativeOrAbsolute);
                version  = HttpVersion.Unknown;
                return ValidationCode.RequestUnknown;
            }
            
            if (elements.Length > 1)
            {
                var uri_success = Uri.TryCreate(elements[1], UriKind.RelativeOrAbsolute, out resource!);
                if (!uri_success)
                {
                    version = HttpVersion.Unknown;
                    return ValidationCode.InvalidResource;
                }
            }
            else
            {
                resource = new Uri("/unknown", UriKind.RelativeOrAbsolute);
                version  = HttpVersion.Unknown;
                return ValidationCode.NoResource;
            }
            
            if (elements.Length > 2)
            {
                version = HttpVersionUtils.Parse(elements[2]);
                if (version == HttpVersion.Unknown)
                {
                    return ValidationCode.UnknownVersion;
                }
            }
            else
            {
                version = HttpVersion.Unknown;
                return ValidationCode.NoVersion;
            }
            
            return ValidationCode.Valid;
        }
        
        public override string ToString()
        {
            return ToHttp();
        }
        
        public enum ValidationCode
        {
            Unknown         = 0,
            Valid           = 1,
            RequestUnknown  = 2,
            NoResource      = 3,
            InvalidResource = 4,
            NoVersion       = 5,
            UnknownVersion  = 6,
        }
    }
}
