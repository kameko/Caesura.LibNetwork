
namespace Caesura.LibNetwork
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    
    public interface IHttpServer : IDisposable
    {
        
        void Start();
        void Stop();
    }
}
