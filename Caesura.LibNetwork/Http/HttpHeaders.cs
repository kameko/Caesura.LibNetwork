
namespace Caesura.LibNetwork.Http
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    
    public class HttpHeaders : IHttpHeaders
    {
        private TriStateValidation is_valid;
        private List<IHttpHeader> Headers;
        
        public bool IsValid    => CheckIsValid();
        public int Count       => Headers.Count;
        public bool HasHeaders => Headers.Count > 0;
        
        public HttpHeaders()
        {
            Headers = new List<IHttpHeader>();
        }
        
        public HttpHeaders(IEnumerable<IHttpHeader> headers)
        {
            Headers = new List<IHttpHeader>(headers);
        }
        
        public string ToHttp()
        {
            var sb = new StringBuilder();
            foreach (var header in Headers)
            {
                sb.Append(header.ToHttp());
            }
            return sb.ToString();
        }
        
        public byte[] ToBytes()
        {
            var http  = ToHttp();
            var bytes = Encoding.UTF8.GetBytes(http);
            return bytes;
        }
        
        public void Add(IHttpHeader header)
        {
            Headers.Add(header);
            is_valid = TriStateValidation.NotSet;
        }
        
        public IEnumerable<IHttpHeader> GetAllValid()
        {
            return Headers.Where(x => x.IsValid);
        }
        
        public IEnumerable<IHttpHeader> GetAllInvalid()
        {
            return Headers.Where(x => !x.IsValid);
        }
        
        public IHttpHeader GetByName(string name)
        {
            return Headers.Find(x => x.CompareName(name))!;
        }
        
        public IEnumerable<IHttpHeader> GetAll()
        {
            return new List<IHttpHeader>(Headers);
        }
        
        public IHttpHeader this[int index]  
        {  
            get { return Headers[index]; }  
            set { Headers.Insert(index, value); }  
        } 

        public IEnumerator<IHttpHeader> GetEnumerator()
        {
            return Headers.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
        
        private bool CheckIsValid()
        {
            if (is_valid == TriStateValidation.NotSet)
            {
                var valid = Headers.All(x => x.IsValid);
                is_valid  = valid ? TriStateValidation.Valid : TriStateValidation.Invalid;
            }
            return is_valid == TriStateValidation.Valid;
        }
        
        private enum TriStateValidation
        {
            NotSet  = 0,
            Valid   = 1,
            Invalid = 2,
        }
    }
}
