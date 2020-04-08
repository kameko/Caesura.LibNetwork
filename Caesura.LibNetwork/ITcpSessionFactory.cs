
namespace Caesura.LibNetwork
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    
    public interface ITcpSessionFactory
    {
        ITcpSession AcceptTcpConnection();
        void Start();
        void Stop();
    }
}
