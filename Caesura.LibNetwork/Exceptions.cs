
namespace Caesura.LibNetwork
{
    using System;
    using System.Runtime.Serialization;
    
    /* DEFAULT: COPY, PASTE AND RENAME
    [Serializable]
    public class MyException : Exception
    {
        public MyException() { }
        public MyException(string message) : base(message) { }
        public MyException(string message, Exception inner) : base(message, inner) { }
        protected MyException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
    */
    
    [Serializable]
    public class TcpSessionNotActiveException : Exception
    {
        public TcpSessionNotActiveException() { }
        public TcpSessionNotActiveException(string message) : base(message) { }
        public TcpSessionNotActiveException(string message, Exception inner) : base(message, inner) { }
        protected TcpSessionNotActiveException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
    
    [Serializable]
    public class UnreliableConnectionException : Exception
    {
        public UnreliableConnectionException() { }
        public UnreliableConnectionException(string message) : base(message) { }
        public UnreliableConnectionException(string message, Exception inner) : base(message, inner) { }
        protected UnreliableConnectionException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
