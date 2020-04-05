
namespace Caesura.LibNetwork
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    
    internal interface IHttpServer : IDisposable
    {
        Task StartAsync();
        void Start();
        void Stop();
    }
}
