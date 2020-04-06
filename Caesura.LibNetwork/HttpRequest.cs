
namespace Caesura.LibNetwork
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    
    public class HttpRequest
    {
        public HttpRequestKind Kind { get; private set; }
        public Uri Resource { get; private set; }
        public HttpVersion Version { get; private set; }
        public bool IsValid { get; private set; }
        
        public HttpRequest()
        {
            Resource = new Uri("/unknown", UriKind.RelativeOrAbsolute);
        }
        
        public HttpRequest(string line)
        {
            IsValid  = TryValidate(line, out var kind, out var resource, out var version);
            Kind     = kind;
            Resource = resource;
            Version  = version;
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
            var elements = request.Split(' ');
            
            kind = elements.Length > 0 ? ParseHttpRequestKind(elements[0]) : HttpRequestKind.Unknown;
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
                    version  = HttpVersion.Unknown;
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
                version = ParseHttpVersion(elements[2]);
                if (version == HttpVersion.Unknown)
                {
                    return ValidationCode.UnknownVersion;
                }
            }
            else
            {
                version  = HttpVersion.Unknown;
                return ValidationCode.NoVersion;
            }
            
            return ValidationCode.Valid;
        }
        
        public static HttpRequestKind ParseHttpRequestKind(string request)
        {
            var success = Enum.TryParse<HttpRequestKind>(request, true, out var result);
            return success ? result : HttpRequestKind.Unknown;
        }
        
        public static HttpVersion ParseHttpVersion(string ver)
        {
            return ver.ToUpper() switch
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
