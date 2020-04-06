
namespace Caesura.LibNetwork
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Text.Json;
    
    public class HttpBody
    {
        internal string raw_body;
        
        public string Body => raw_body;
        public bool HasBody => !(string.IsNullOrEmpty(raw_body) || string.IsNullOrWhiteSpace(raw_body));
        public bool IsValid => true;
        
        public HttpBody()
        {
            raw_body = string.Empty;
        }
        
        public HttpBody(string body)
        {
            raw_body = Sanitize(body);
        }
        
        public DeserializationResult<T> Deserialize<T>()
        {
            var options = new JsonSerializerOptions()
            {
                // TODO: consider deserializer options
            };
            return Deserialize<T>(options);
        }
        
        public DeserializationResult<T> Deserialize<T>(JsonSerializerOptions options)
        {
            if (!HasBody)
            {
                return new DeserializationResult<T>(DeserializationCode.NoBody);
            }
            if (!IsValid)
            {
                return new DeserializationResult<T>(DeserializationCode.BodyNotValid);
            }
            
            try
            {
                var item = JsonSerializer.Deserialize<T>(raw_body, options);
                return new DeserializationResult<T>(item);
            }
            catch (JsonException je)
            {
                return new DeserializationResult<T>(DeserializationCode.DeserializationError, je);
            }
            catch (Exception e)
            {
                return new DeserializationResult<T>(DeserializationCode.UnknownError, e);
            }
        }
        
        private string Sanitize(string body)
        {
            if (body.StartsWith("\r\n"))
            {
                body = body.Substring(2);
            }
            return body;
        }
        
        public override string ToString()
        {
            return raw_body;
        }
        
        public enum DeserializationCode
        {
            Unknown              = 0,
            Ok                   = 1,
            NoBody               = 2,
            BodyNotValid         = 3,
            UnknownError         = 4,
            DeserializationError = 5,
        }
        
        public class DeserializationResult<T>
        {
            public DeserializationCode Code { get; set; }
            public T Item { get; set; }
            public Exception? Error { get; set; }
            
            public bool IsOk => Code == DeserializationCode.Ok;
            
            public DeserializationResult(T item)
            {
                Code  = DeserializationCode.Ok;
                Item  = item;
            }
            
            public DeserializationResult(DeserializationCode code)
            {
                Code = code;
                Item = default!;
            }
            
            public DeserializationResult(DeserializationCode code, Exception exception)
            {
                Code  = code;
                Item  = default!;
                Error = exception;
            }
        }
    }
}
