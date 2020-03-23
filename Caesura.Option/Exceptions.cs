
namespace Caesura.Option
{
    using System;
    using System.Runtime.Serialization;
    
    [Serializable]
    public class NoneOptionException : Exception
    {
        public NoneOptionException() { }
        public NoneOptionException(string message) : base(message) { }
        public NoneOptionException(string message, Exception inner) : base(message, inner) { }
        protected NoneOptionException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
    
    [Serializable]
    public class UnhandledResultErrorException : Exception
    {
        public UnhandledResultErrorException() { }
        public UnhandledResultErrorException(string message) : base(message) { }
        public UnhandledResultErrorException(string message, Exception inner) : base(message, inner) { }
        protected UnhandledResultErrorException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
    
    [Serializable]
    public class ResultNotOkException : Exception
    {
        public ResultNotOkException() { }
        public ResultNotOkException(string message) : base(message) { }
        public ResultNotOkException(string message, Exception inner) : base(message, inner) { }
        protected ResultNotOkException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
    
    [Serializable]
    public class ResultNotErrorException : Exception
    {
        public ResultNotErrorException() { }
        public ResultNotErrorException(string message) : base(message) { }
        public ResultNotErrorException(string message, Exception inner) : base(message, inner) { }
        protected ResultNotErrorException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
    
    [Serializable]
    public class ResultGenericArgumentsMatchException : Exception
    {
        public ResultGenericArgumentsMatchException() { }
        public ResultGenericArgumentsMatchException(string message) : base(message) { }
        public ResultGenericArgumentsMatchException(string message, Exception inner) : base(message, inner) { }
        protected ResultGenericArgumentsMatchException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
