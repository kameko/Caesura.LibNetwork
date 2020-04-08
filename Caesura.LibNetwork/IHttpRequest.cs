
namespace Caesura.LibNetwork
{
    using System;
    
    public interface IHttpRequest
    {
        HttpRequestKind Kind { get; }
        Uri Resource { get; }
        HttpVersion Version { get; }
        IHttpMessage Message { get; }
        bool IsValid { get; }
        
        string ToHttp();
        byte[] ToBytes();
    }
}
