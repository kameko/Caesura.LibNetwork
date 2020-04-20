
namespace Caesura.LibNetwork.Http
{
    public enum HttpRequestValidationCode
    {
        Unknown         = 0,
        Valid           = 1,
        RequestUnknown  = 2,
        NoResource      = 3,
        InvalidResource = 4,
        NoVersion       = 5,
        UnknownVersion  = 6,
    }
    
    public interface IHttpRequest
    {
        HttpRequestKind Kind { get; }
        Resource Resource { get; }
        HttpVersion Version { get; }
        IHttpMessage Message { get; }
        HttpRequestValidationCode Validation { get; }
        bool IsValid { get; }
        
        string ToHttp();
        byte[] ToBytes();
    }
}
