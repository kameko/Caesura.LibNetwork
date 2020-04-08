
namespace Caesura.LibNetwork.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Text;
    using System.IO;
    using Xunit;
    using Xunit.Abstractions;
    using Http;
    
    public class HttpServerTests
    {
        private readonly ITestOutputHelper output;
        
        public HttpServerTests(ITestOutputHelper output)
        {
            this.output = output;
            Write(string.Empty);
        }
        
        private void Write(string line)
        {
            output.WriteLine(line);
        }
        
        private IEnumerable<int> basic_test_port_gen_1()
        {
            yield return 1;
            yield return 2;
            yield return 3;
        }
        
        [Fact]
        public void basic_test_1()
        {
            var config = new LibNetworkConfig()
            {
                Factories = new LibNetworkFactories()
                {
                    TcpSessionFactoryFactory = (c) => new MockTcpSessionFactory(c, basic_test_port_gen_1)
                }
            };
        }
    }
}
