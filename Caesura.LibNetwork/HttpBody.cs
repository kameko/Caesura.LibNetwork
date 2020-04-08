
namespace Caesura.LibNetwork
{
    using System;
    using System.Text;
    using System.Text.Json;
    
    public class HttpBody : IHttpBody
    {
        internal string raw_body;
        
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
        
        public string ToHttp()
        {
            return raw_body;
        }
        
        public byte[] ToBytes()
        {
            var http  = ToHttp();
            var bytes = Encoding.UTF8.GetBytes(http);
            return bytes;
        }
        
        public bool TryDeserialize<T>(out T item)
        {
            var result = Deserialize<T>();
            item = result.IsOk ? result.Item : default!;
            return result.IsOk;
        }
        
        public T DeserializeOrThrow<T>()
        {
            var result = Deserialize<T>();
            if (!result.IsOk)
            {
                if (result.Error is null)
                {
                    throw new InvalidOperationException("Deserialization was not successful.");
                }
                else
                {
                    throw result.Error;
                }
            }
            else
            {
                return result.Item;
            }
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
            private T _item;
            
            public DeserializationCode Code { get; set; }
            public T Item
            {
                get => Code == DeserializationCode.Ok 
                    ? _item
                    : throw new InvalidOperationException("Deserialization result not ok.");
                set => _item = value;
            }
            public Exception? Error { get; set; }
            
            public bool IsOk => Code == DeserializationCode.Ok;
            
            public DeserializationResult(T item)
            {
                _item = item;
                Code  = DeserializationCode.Ok;
            }
            
            public DeserializationResult(DeserializationCode code)
            {
                _item  = default!;
                Code = code;
            }
            
            public DeserializationResult(DeserializationCode code, Exception exception)
            {
                _item  = default!;
                Code  = code;
                Error = exception;
            }
        }
    }
}
