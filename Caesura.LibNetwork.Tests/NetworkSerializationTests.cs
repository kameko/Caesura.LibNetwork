
namespace Caesura.LibNetwork.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Text;
    using System.IO;
    using Xunit;
    using Xunit.Abstractions;
    
    public class NetworkSerializationTests
    {
        private readonly ITestOutputHelper output;
        
        public NetworkSerializationTests(ITestOutputHelper output)
        {
            this.output = output;
            Write(string.Empty);
        }
        
        private void Write(string line)
        {
            output.WriteLine(line);
        }
        
        [Fact]
        public void request_deserialization_1()
        {
            var memstream = new MemoryStream();
            var input     = new StreamWriter(memstream, Encoding.UTF8);
            var output    = new StreamReader(memstream, Encoding.UTF8);
            input.AutoFlush = true;
            
            var request1  = new HttpRequest(
                "GET /some/resource.json HTTP/1.1",
                new HttpMessage(
                    new HttpHeaders()
                    {
                        new HttpHeader("Accept-Language", "en-US"),
                        new HttpHeader("Host", "localhost:4899"),
                        new HttpHeader("User-Agent", "Solace NT"),
                    },
                    new HttpBody(
                        "{\r\n\t\"message\": \"hello, world!\"\r\n}"
                    )
                )
            );
            Assert.True(request1.IsValid);
            
            var http1 = request1.ToHttp();
            Write(http1);
            input.Write(http1);
            memstream.Position = 0; // Thank you MemoryStream, I hate you :) 
            
            var token  = new CancellationTokenSource(5_000);
            
            var request2 = HttpRequest.FromStream(output, 100, token.Token);
            
            Assert.True(request2.IsValid);
        }
    }
}
