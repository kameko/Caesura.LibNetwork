
namespace Caesura.LibNetwork.Http
{
    public interface IHttpMessage
    {
        IHttpHeaders Headers { get; }
        IHttpBody Body { get; }
        bool IsValid { get; }
        int HeaderCount { get; }
        bool HasHeaders { get; }
        bool HasBody { get; }
        
        string ToHttp();
        byte[] ToBytes();
    }
}
