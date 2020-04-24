
namespace Caesura.LibNetwork.TestConsumer
{
    using System;
    using System.Threading.Tasks;
    using Http;
    
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting test server...");
            
            int port = 4988;
            
            /*
            while (true)
            {
                Console.Write("PORT> ");
                var port_str = Console.ReadLine();
                var success = int.TryParse(port_str, out port);
                if (success && port > 0)
                {
                    Console.WriteLine($"PORT SET TO {port}.");
                    break;
                }
                else
                {
                    Console.WriteLine($"INVALID INPUT '{port_str}'. PORT MUST BE A POSITIVE INTEGER.");
                }
            }
            //*/
            
            var config = new LibNetworkConfig()
            {
                Port = port,
                Http = new HttpConfig()
                {
                    SessionTimeout = TimeSpan.FromSeconds(20),
                },
            };
            
            var server = new HttpServer(config);
            
            server.OnNewConnection += http_session =>
            {
                http_session.OnUnhandledException += e =>
                {
                    Console.WriteLine($"SESSION UNHANDLED EXCEPTION: {e}");
                    return Task.CompletedTask;
                };
                
                http_session.OnInvalidRequest += (req, session) =>
                {
                    Console.WriteLine($"INVALID REQUEST {req}");
                    return Task.CompletedTask;
                };
                
                http_session.OnTimeoutDisconnect += session =>
                {
                    Console.WriteLine("SESSION TIMED OUT");
                    return Task.CompletedTask;
                };
                
                http_session.OnAnyValidRequest += (req, session) =>
                {
                    Console.WriteLine($"GOT REQUEST {req}");
                    return Task.CompletedTask;
                };
                
                return Task.CompletedTask;
            };
            
            server.OnUnhandledException += e =>
            {
                Console.WriteLine($"SERVER UNHANDLED EXCEPTION: {e}");
                return Task.CompletedTask;
            };
            
            server.OnSessionException += e =>
            {
                Console.WriteLine($"SERVER SESSION EXCEPTION: {e}");
                return Task.CompletedTask;
            };
            
            server.OnSocketException += num =>
            {
                Console.WriteLine($"SERVER SOCKET ERROR: {num}");
                return Task.CompletedTask;
            };
            
            await server.StartAsync();
            
            Console.WriteLine("END OF EXECUTION.");
            Console.ReadLine();
        }
    }
}
