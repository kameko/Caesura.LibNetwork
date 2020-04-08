
namespace Caesura.LibNetwork
{
    public interface IHttpHeader
    {
        string Header { get; }
        string Name { get; }
        string Body { get; }
        bool IsValid { get; }
        
        string ToHttp();
        byte[] ToBytes();
        string ToHttpNoNewline();
        bool CompareName(string name);
        
    }
}
