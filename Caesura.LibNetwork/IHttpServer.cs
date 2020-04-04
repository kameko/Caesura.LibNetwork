
namespace Caesura.LibNetwork
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    
    internal interface IHttpServer : IDisposable
    {
        
        void Start();
        void Stop();
    }
}
