
namespace Caesura.LibNetwork
{
    public interface IHttpBody
    {
        bool HasBody { get; }
        bool IsValid { get; }
        
        string ToHttp();
        byte[] ToBytes();
        bool TryDeserialize<T>(out T item);
        T DeserializeOrThrow<T>();
    }
}
