
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
        public Resource Resource { get; private set; }
        public HttpVersion Version { get; private set; }
        public IHttpMessage Message { get; set; }
        
        private HttpRequestValidationCode validation_code;
        public HttpRequestValidationCode Validation => validation_code;
        public bool IsValid => validation_code == HttpRequestValidationCode.Valid && Resource.IsValid && Message.IsValid;
        
        private HttpRequest()
        {
            Resource = Resource.Index;
            Message  = new HttpMessage();
        }
        
        private HttpRequest(string line, IHttpMessage message)
        {
            validation_code = Validate(line, out var kind, out var resource, out var version);
            Kind            = kind;
            Resource        = resource;
            Version         = version;
            Message         = message;
        }
        
        public HttpRequest(HttpRequestKind kind, Resource resource, HttpVersion version, IHttpMessage message)
        {
            Kind            = kind;
            Resource        = resource;
            Version         = version;
            Message         = message;
            validation_code = HttpRequestValidationCode.Valid;
        }
        
        public HttpRequest(HttpRequestKind kind, string resource, HttpVersion version, IHttpMessage message)
        {
            Kind            = kind;
            Resource        = new Resource(resource);
            Version         = version;
            Message         = message;
            validation_code = HttpRequestValidationCode.Valid;
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
                + Resource.Representation
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
        
        public static bool TryValidate(string request, out HttpRequestKind kind, out Resource resource, out HttpVersion version)
        {
            var result = Validate(request, out kind, out resource, out version);
            return result == HttpRequestValidationCode.Valid;
        }
        
        public static void ValidateOrThrow(string request, out HttpRequestKind kind, out Resource resource, out HttpVersion version)
        {
            var result = Validate(request, out kind, out resource, out version);
            if (result != HttpRequestValidationCode.Valid)
            {
                throw new InvalidHttpRequestException(result);
            }
        }
        
        public static HttpRequestValidationCode Validate(string request, out HttpRequestKind kind, out Resource resource, out HttpVersion version)
        {
            var elements = request?.Split(' ') ?? new string[0];
            
            kind = elements.Length > 0 ? HttpRequestKindUtils.ParseHttpRequestKind(elements[0]) : HttpRequestKind.Unknown;
            if (kind == HttpRequestKind.Unknown)
            {
                resource = new Resource();
                version  = HttpVersion.Unknown;
                return HttpRequestValidationCode.RequestUnknown;
            }
            
            if (elements.Length > 1)
            {
                resource = new Resource(elements[1]);
                if (!resource.IsValid)
                {
                    version = HttpVersion.Unknown;
                    return HttpRequestValidationCode.InvalidResource;
                }
            }
            else
            {
                resource = new Resource();
                version  = HttpVersion.Unknown;
                return HttpRequestValidationCode.NoResource;
            }
            
            if (elements.Length > 2)
            {
                version = HttpVersionUtils.Parse(elements[2]);
                if (version == HttpVersion.Unknown)
                {
                    return HttpRequestValidationCode.UnknownVersion;
                }
            }
            else
            {
                version = HttpVersion.Unknown;
                return HttpRequestValidationCode.NoVersion;
            }
            
            return HttpRequestValidationCode.Valid;
        }
        
        public override string ToString()
        {
            return ToHttp();
        }
    }
}
