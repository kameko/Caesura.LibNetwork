
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
        
        [Fact]
        public async Task basic_test_1()
        {
            var got_request  = false;
            var got_response = false;
            
            var config1 = new LibNetworkConfig()
            {
                Port = 1,
            };
            
            var mock_session_factory1 = new MockTcpSessionFactory(config1);
            config1.Factories.TcpSessionFactoryFactory = c => mock_session_factory1;
            
            var config2 = new LibNetworkConfig()
            {
                Port = 2,
            };
            var mock_session_factory2 = new MockTcpSessionFactory(config2);
            config2.Factories.TcpSessionFactoryFactory = c => mock_session_factory2;
            
            var server1 = new HttpServer(config1);
            var server2 = new HttpServer(config2);
            
            server1.OnGET += async (req, session) =>
            {
                got_response = true;
                
                var response = new HttpResponse(
                    HttpVersion.HTTP1_1,
                    HttpStatusCode.OK,
                    new HttpMessage(
                        new HttpHeaders()
                        {
                            new HttpHeader("Accept-Language", "en-US"),
                            new HttpHeader("Host", "localhost:4899"),
                            new HttpHeader("User-Agent", "Solace NT"),
                        },
                        new HttpBody(
                            "{\r\n    \"message\": \"hello back to you!\"\r\n}"
                        )
                    )
                );
                await session.Respond(response);
            };
            
            server2.OnGET += async (req, session) =>
            {
                got_request = true;
                
                var response = new HttpResponse(
                    HttpVersion.HTTP1_1,
                    HttpStatusCode.OK,
                    new HttpMessage(
                        new HttpHeaders()
                        {
                            new HttpHeader("Accept-Language", "en-US"),
                            new HttpHeader("Host", "localhost:4899"),
                            new HttpHeader("User-Agent", "Solace NT"),
                        },
                        new HttpBody(
                            "{\r\n    \"message\": \"hello back to you!\"\r\n}"
                        )
                    )
                );
                await session.Respond(response);
            };
            
            server1.OnAnyValidRequest += (req, session) =>
            {
                if (req.Kind != HttpRequestKind.GET)
                {
                    throw new InvalidOperationException("Should not get here.");
                }
                return Task.CompletedTask;
            };
            
            server2.OnAnyValidRequest += (req, session) =>
            {
                if (req.Kind != HttpRequestKind.GET)
                {
                    throw new InvalidOperationException("Should not get here.");
                }
                return Task.CompletedTask;
            };
            
            server1.Start();
            server2.Start();
            
            var response = new HttpRequest(
                HttpRequestKind.GET,
                "/some/resource.json",
                HttpVersion.HTTP1_1,
                new HttpMessage(
                    new HttpHeaders()
                    {
                        new HttpHeader("Accept-Language", "en-US"),
                        new HttpHeader("Host", "localhost:4899"),
                        new HttpHeader("User-Agent", "Solace NT"),
                    },
                    new HttpBody(
                        "{\r\n    \"message\": \"hello, world!\"\r\n}"
                    )
                )
            );
            
            var stream1 = new MemoryStream();
            mock_session_factory2.SimulateConnection(stream1, port: 1);
            await server1.SendRequest("localhost", 2, response);
            
            await Task.Delay(5_000);
            
            server1.Stop();
            server2.Stop();
            
            Assert.True(got_request);
            Assert.True(got_response);
        }
    }
}
