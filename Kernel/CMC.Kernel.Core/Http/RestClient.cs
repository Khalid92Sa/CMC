using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using CMC.Kernel.Core.Wrappers;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CMC.Kernel.Core.Http
{
    public class RestClient : IRestClient
    {
        /// <summary>
        /// The HTTP client factory
        /// </summary>
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private HttpClient _client;
        private Dictionary<string, string> _headers;

        public RestClient(IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _client = _httpClientFactory.CreateClient();
            _headers = new Dictionary<string, string>();
        }

        public string this[string key]
        {
            get => _headers[key];
            set => _headers[key] = value;
        }

        public void AddBasicAuthentication(string username, string password)
        {
            var byteArray = Encoding.ASCII.GetBytes($"{username}:{password}");
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
        }

        public void AddBearerTokenAuthentication()
        {
            _httpContextAccessor.HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues token);
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.ToString().Replace("Bearer ", ""));
        }

        public void AddBearerTokenAuthentication(string token)
        {
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        public void ClearHeader()
        {
            _client.DefaultRequestHeaders.Clear();
        }

        /// <inheritdoc />
        public async Task<Response<T>> GetAsync<T>(string url, Dictionary<string, string> headers = null)
        {
            return await SendAsync<T>(client => client.GetAsync(url), headers);
        }

        public async Task<Response<T>> PostAsync<T>(string url, object model, Dictionary<string, string> headers = null)
        {
            return await PostAsync<T>(url, new StringContent(JsonConvert.SerializeObject(model), System.Text.Encoding.UTF8, "application/json"), headers);
        }

        /// <inheritdoc />
        public async Task<Response<T>> PostAsync<T>(string url, HttpContent content, Dictionary<string, string> headers = null)
        {
            return await SendAsync<T>(client => client.PostAsync(url, content), headers);
        }

        public async Task<Response> PostAsync(string url, object model)
        {
            return await PostAsync(url, new StringContent(JsonConvert.SerializeObject(model), System.Text.Encoding.UTF8, "application/json"));
        }

        public async Task<Response> PostAsync(string url, HttpContent content)
        {
            return await SendAsync<object>(client => client.PostAsync(url, content));
        }

        private async Task<Response<T>> SendAsync<T>(Func<HttpClient, Task<HttpResponseMessage>> senderFunc, Dictionary<string, string> headers = null)
        {
            if (_headers != null)
            {
                foreach (var header in _headers)
                    _client.DefaultRequestHeaders.Add(header.Key, header.Value);
            }

            if (headers != null)
            {
                foreach (var header in headers)
                    _client.DefaultRequestHeaders.Add(header.Key, header.Value);
            }

            var res = await senderFunc(_client);
            if (res.IsSuccessStatusCode)
            {
                if (typeof(T) == typeof(String))
                    return new Response<T> { Succeeded = true, Message = await res.Content.ReadAsStringAsync() };
                else
                {
                    return new Response<T> { Succeeded = true, Data = JsonConvert.DeserializeObject<T>(await res.Content.ReadAsStringAsync()) };

                }
                //return new Response<T> { Succeeded = true, Data = JsonConvert.DeserializeObject<T>(await res.Content.ReadAsStringAsync()) };
            }
            return new Response<T> { StatusCode = (int)res.StatusCode, Message = await res.Content.ReadAsStringAsync() };
        }
    }
}
