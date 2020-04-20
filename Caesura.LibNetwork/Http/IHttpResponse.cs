
namespace Caesura.LibNetwork.Http
{
    public enum HttpResponseValidationCode
        {
            Unknown        = 0,
            Valid          = 1,
            UnknownVersion = 2,
            NoVersion      = 3,
            NoStatus       = 4,
            StatusNotInt   = 5,
            UnknownStatus  = 6,
        }
    
    public interface IHttpResponse
    {
        HttpVersion Version { get; }
        HttpStatusCode StatusCode { get; }
        IHttpMessage Message { get; }
        bool IsInformationalStatusCode { get; }
        bool IsSuccessStatusCode { get; }
        bool IsRedirectionStatusCode { get; }
        bool IsClientErrorStatusCode { get; }
        bool IsServerErrorStatusCode { get; }
        HttpResponseValidationCode Validation { get; }
        bool IsValid { get; }
        
        string ToHttp();
        byte[] ToBytes();
    }
}
