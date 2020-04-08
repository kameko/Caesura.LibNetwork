
namespace Caesura.LibNetwork
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    
    public interface ITcpListener
    {
        ITcpSession AcceptTcpConnection();
        void Start();
        void Stop();
    }
}
