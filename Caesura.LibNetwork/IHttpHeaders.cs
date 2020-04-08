
namespace Caesura.LibNetwork
{
    using System.Collections.Generic;
    
    public interface IHttpHeaders : IEnumerable<IHttpHeader>
    {
        bool IsValid { get; }
        int Count { get; }
        bool HasHeaders { get; }
        
        string ToHttp();
        byte[] ToBytes();
        void Add(IHttpHeader header);
        IEnumerable<IHttpHeader> GetAllValid();
        IEnumerable<IHttpHeader> GetAllInvalid();
        IHttpHeader GetByName(string name);
        IEnumerable<IHttpHeader> GetAll();
    }
}
