
namespace Caesura.LibNetwork.Http
{
    using System;
    
    public interface IHttpRequest
    {
        HttpRequestKind Kind { get; }
        Resource Resource { get; }
        HttpVersion Version { get; }
        IHttpMessage Message { get; }
        bool IsValid { get; }
        
        string ToHttp();
        byte[] ToBytes();
    }
}
