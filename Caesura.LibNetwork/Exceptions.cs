
namespace Caesura.LibNetwork
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
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
    public class InvalidHttpHeaderException : Exception
    {
        public HttpHeader.ValidationCode ValidationCode { get; private set; }
        
        public InvalidHttpHeaderException(HttpHeader.ValidationCode validation) : this(validation.ToString())
        {
            ValidationCode = validation;
        }
        
        public InvalidHttpHeaderException() { }
        public InvalidHttpHeaderException(string message) : base(message) { }
        public InvalidHttpHeaderException(string message, Exception inner) : base(message, inner) { }
        protected InvalidHttpHeaderException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
    
    [Serializable]
    public class InvalidHttpRequestException : Exception
    {
        public HttpRequest.ValidationCode ValidationCode { get; private set; }
        
        public InvalidHttpRequestException(HttpRequest.ValidationCode validation) : this(validation.ToString())
        {
            ValidationCode = validation;
        }
        
        public InvalidHttpRequestException() { }
        public InvalidHttpRequestException(string message) : base(message) { }
        public InvalidHttpRequestException(string message, Exception inner) : base(message, inner) { }
        protected InvalidHttpRequestException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
