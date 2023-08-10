using CMC.Kernel.Core.Wrappers;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CMC.Kernel.Core.Http
{
    public interface IRestClient
    {
        string this[string key]
        {
            get;
            set;
        }
        void AddBasicAuthentication(string username, string password);
        /// <summary>
        /// Adds the bearer token authentication(reads it from the current httpcontext request).
        /// </summary>
        void AddBearerTokenAuthentication();
        void AddBearerTokenAuthentication(string token);
        void ClearHeader();
        Task<Response<T>> GetAsync<T>(string url, Dictionary<string, string> headers = null);
        Task<Response<T>> PostAsync<T>(string url, HttpContent content, Dictionary<string, string> headers = null);
        Task<Response<T>> PostAsync<T>(string url, object model, Dictionary<string, string> headers = null);
        Task<Response> PostAsync(string url, HttpContent content);
        Task<Response> PostAsync(string url, object model);
    }
}
