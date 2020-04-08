
namespace Caesura.LibNetwork.Http
{
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
        bool IsValid { get; }
        
        string ToHttp();
        byte[] ToBytes();
    }
}
