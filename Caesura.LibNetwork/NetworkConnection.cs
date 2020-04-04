
namespace Caesura.LibNetwork
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Net;
    using System.Net.Http;
    using System.Text.Json;
    
    // TODO: Some sort of event system to handle network errors,
    // in addition to the returned response code from each request.
    // TODO: Some sort of object full of events that this class can
    // use for hooking a logger to.
    
    public class NetworkConnection : IDisposable
    {
        private LibNetworkConfig Config;
        private bool canceled;
        private bool disposed;
        private CancellationTokenSource CancelTokenSource;
        private HttpClient HttpNetwork;
        
        internal NetworkConnection()
        {
            canceled          = false;
            disposed          = false;
            CancelTokenSource = new CancellationTokenSource();
            HttpNetwork       = null!;
            Config            = null!;
        }
        
        private void Init(LibNetworkConfig config)
        {
            Config = config;
            if (config.HttpHandler is null)
            {
                HttpNetwork = new HttpClient();
            }
            else
            {
                HttpNetwork = new HttpClient(config.HttpHandler, config.DisposeHttpHandler);
            }
        }
        
        /// <summary>
        /// Create a new instance of a network connection with the default configuration.
        /// </summary>
        /// <returns></returns>
        public static NetworkConnection Create() => Create(LibNetworkConfig.GetDefault());
        
        /// <summary>
        /// Create a new instance of a network connection with the provided configuration.
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static NetworkConnection Create(LibNetworkConfig config)
        {
            var nc = new NetworkConnection();
            nc.Init(config);
            return nc;
        }
        
        /// <summary>
        /// GET a specific object.
        /// </summary>
        /// <param name="uri"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<NetworkResponse<T>> Get<T>(Uri uri)
        {
            TestObjectValidity();
            
            var rsp = await HttpNetwork.GetAsync(uri, Config.CompletionOption, CancelTokenSource.Token);
            
            if (rsp is null)
            {
                return new NetworkResponse<T>()
                {
                    StatusCode          = HttpStatusCode.ServiceUnavailable,
                    IsSuccessStatusCode = false,
                };
            }
            
            var result = new NetworkResponse<T>()
            {
                StatusCode          = rsp.StatusCode,
                IsSuccessStatusCode = rsp.IsSuccessStatusCode,
            };
            
            if (rsp.IsSuccessStatusCode)
            {
                var raw_content = await rsp.Content.ReadAsByteArrayAsync();
                
                try
                {
                    var item = JsonSerializer.Deserialize<T>(raw_content, Config.JsonOptions);
                    result.Entity = item;
                }
                catch (JsonException)
                {
                    // TODO: invoke error callback. if it's null, rethrow instead.
                    throw;
                }
            }
            else
            {
                // TODO: log failure.
            }
            
            return result;
        }
        
        /// <summary>
        /// DELETE the object at the destination URI. This requires
        /// a generic argument to ensure type safety.
        /// </summary>
        /// <param name="uri"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public Task<NetworkResponse> Delete<T>(Uri uri)
        {
            TestObjectValidity();
            throw new NotImplementedException();
        }
        
        public Task<NetworkResponse> Patch<T, V>(Uri uri, T item_to_modify, string field_name_to_modify, V set_field_to_item)
        {
            TestObjectValidity();
            throw new NotImplementedException();
        }
        
        /// <summary>
        /// PUT (create and name, or update) an object at the
        /// destination URI. 
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="item"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public Task<NetworkResponse> Put<T>(Uri uri, T item)
        {
            TestObjectValidity();
            throw new NotImplementedException();
        }
        
        /// <summary>
        /// POST (create) a new object at the destination URI and let
        /// the server decide the name, return an error if it exists.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="item"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public Task<NetworkResponse> Post<T>(Uri uri, T item)
        {
            TestObjectValidity();
            throw new NotImplementedException();
        }
        
        /// <summary>
        /// Alias for Get(uri).
        /// </summary>
        /// <param name="uri"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public Task<NetworkResponse<T>> Read<T>(Uri uri) => Get<T>(uri);
        
        /// <summary>
        /// Alias for Put(uri, item)
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="item"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public Task<NetworkResponse> Write<T>(Uri uri, T item) => Put(uri, item);
        
        private void TestObjectValidity()
        {
            if (CancelTokenSource.IsCancellationRequested || disposed)
            {
                throw new InvalidOperationException("NetworkConnection has been disposed.");
            }
        }
        
        /// <summary>
        /// Cancel the connection. Note that calling Dispose will
        /// also cancel. It is safe to call Cancel and then Dispose.
        /// </summary>
        public void Cancel()
        {
            if (!canceled)
            {
                CancelTokenSource.Cancel();
                HttpNetwork.CancelPendingRequests();
                canceled = true;
            }
        }
        
        /// <summary>
        /// Cancel and dispose of the connection.
        /// </summary>
        public void Dispose()
        {
            if (!disposed)
            {
                Cancel();
                HttpNetwork.Dispose();
                disposed = true;
            }
        }
    }
}
