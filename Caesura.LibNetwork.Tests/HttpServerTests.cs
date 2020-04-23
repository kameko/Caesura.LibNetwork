
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
            // TODO: debug. Something is throwing an EndOfStreamException too.
            
            var got_request  = false;
            var got_response = false;
            
            var server1_err_spot = 0;
            var server2_err_spot = 0;
            Exception? server1_exception = null;
            Exception? server2_exception = null;
            
            var config1 = new LibNetworkConfig()
            {
                Port = 1,
            };
            //config1.Http.ThreadPerConnection = false;
            var mock_session_factory1 = new MockTcpSessionFactory(config1);
            config1.Factories.TcpSessionFactoryFactory = _ => mock_session_factory1;
            
            var config2 = new LibNetworkConfig()
            {
                Port = 2,
            };
            //config2.Http.ThreadPerConnection = false;
            var mock_session_factory2 = new MockTcpSessionFactory(config2);
            config2.Factories.TcpSessionFactoryFactory = _ => mock_session_factory2;
            
            var server1 = new HttpServer(config1);
            var server2 = new HttpServer(config2);
            
            server1.OnUnhandledException += e => { server1_exception = e; server1_err_spot = 1; return Task.CompletedTask; };
            server2.OnUnhandledException += e => { server2_exception = e; server2_err_spot = 1; return Task.CompletedTask; };
            server1.OnSessionException   += e => { server1_exception = e; server1_err_spot = 2; return Task.CompletedTask; };
            server2.OnSessionException   += e => { server2_exception = e; server2_err_spot = 2; return Task.CompletedTask; };
            
            server2.OnNewConnection += session =>
            {
                session.OnUnhandledException += e => { server2_exception = e; server2_err_spot = 3; return Task.CompletedTask; };
                session.OnGET += async (req, session) =>
                {
                    got_request = true;
                    
                    var response = new HttpResponse(
                        HttpVersion.HTTP1_1,
                        HttpStatusCode.OK,
                        new HttpMessage(
                            new HttpHeaders()
                            {
                                new HttpHeader("Accept-Language", "en-US"),
                                new HttpHeader("Host", "localhost:2"),
                                new HttpHeader("User-Agent", "Solace NT"),
                            },
                            new HttpBody(
                                "{\r\n    \"message\": \"hello back to you!\"\r\n}"
                            )
                        )
                    );
                    
                    await Task.Delay(1_000);
                    
                    await session.Respond(response);
                };
                
                return Task.CompletedTask;
            };
            
            var server1_task = server1.StartAsync();
            var server2_task = server2.StartAsync();
            
            _ = server1_task.ContinueWith(t => { if (t.IsFaulted) { server1_exception = t.Exception; server1_err_spot = 4; } });
            _ = server2_task.ContinueWith(t => { if (t.IsFaulted) { server2_exception = t.Exception; server2_err_spot = 4; } });
            
            // FIXME: server needs time to boot up
            await Task.Delay(1_000);
            
            var request1 = new HttpRequest(
                HttpRequestKind.GET,
                "/some/resource.json",
                HttpVersion.HTTP1_1,
                new HttpMessage(
                    new HttpHeaders()
                    {
                        new HttpHeader("Accept-Language", "en-US"),
                        new HttpHeader("User-Agent", "Solace NT"),
                    },
                    new HttpBody(
                        "{\r\n    \"message\": \"hello, world!\"\r\n}"
                    )
                )
            );
            var stream1 = new MemoryStream();
            mock_session_factory1.SimulateConnection(stream1, port: 2);
            var session1 = await server1.SendRequest("localhost", 2, request1);
            
            session1.OnUnhandledException += e => { server1_exception = e; server1_err_spot = 5; return Task.CompletedTask; };
            session1.OnGET += (resp, session) =>
            {
                got_response = true;
                return Task.CompletedTask;
            };
            
            await Task.Delay(5_000);
            
            server1.Stop();
            server2.Stop();
            
            if (!(server1_exception is null))
            {
                Write($"Server1 Exception, Spot {server1_err_spot}: " + server1_exception.ToString());
                throw server1_exception;
            }
            if (!(server2_exception is null))
            {
                Write($"Server2 Exception, Spot {server2_err_spot}: " + server2_exception.ToString());
                throw server2_exception;
            }
            
            Assert.True(got_request);
            Assert.True(got_response);
        }
    }
}
