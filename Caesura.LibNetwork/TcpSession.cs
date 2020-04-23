
namespace Caesura.LibNetwork
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.IO;
    using System.Text;
    using System.Net.Sockets;
    
    internal class TcpSession : ITcpSession
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private StreamWriter _writer;
        private StreamReader _reader;
        
        public TcpSessionState State { get; private set; }
        public Guid Id { get; private set; }
        public StreamReader Output => _reader;
        public bool DataAvailable => _stream.DataAvailable && State != TcpSessionState.Closed;
        
        public TcpSession(TcpClient client, int ticks)
        {
            _client        = client;
            _stream        = client.GetStream();
            _writer        = new StreamWriter(_stream, Encoding.UTF8);
            _reader        = new StreamReader(_stream, Encoding.UTF8);
            
            State          = TcpSessionState.Ready;
            Id             = Guid.NewGuid();
        }
        
        internal TcpSession()
        {
            _client        = null!;
            _stream        = null!;
            _writer        = null!;
            _reader        = null!;
            
            State          = TcpSessionState.Closed;
            Id             = Guid.NewGuid();
        }
        
        public async Task Write(string text, CancellationToken token)
        {
            if (State == TcpSessionState.Closed)
            {
                throw new TcpSessionNotActiveException();
            }
            
            await _writer.WriteAsync(text);
            await _writer.FlushAsync();
        }
        
        public void Close()
        {
            State = TcpSessionState.Closed;
            
            _client?.Close();
            _stream?.Close();
            _writer?.Close();
            _reader?.Close();
        }
        
        public void Dispose()
        {
            Close();
        }
    }
}
