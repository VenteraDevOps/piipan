using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Piipan.Shared.Authentication;

namespace Piipan.Shared.Http
{
    /// <summary>
    /// Client for making authorized API calls within the Piipan system
    /// </summary>
    public class AuthorizedJsonApiClient<T> : IAuthorizedApiClient<T>
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly ITokenProvider<T> _tokenProvider;
        private const string _accept = "application/json";

        /// <summary>
        /// Creates a new instance of AuthorizedJsonApiClient
        /// </summary>
        /// <param name="clientFactory">an instance of IHttpClientFactory</param>
        /// <param name="tokenProvider">an instance of ITokenProvider</param>
        public AuthorizedJsonApiClient(IHttpClientFactory clientFactory,
            ITokenProvider<T> tokenProvider)
        {
            _clientFactory = clientFactory;
            _tokenProvider = tokenProvider;
        }

        private async Task<HttpRequestMessage> PrepareRequest(string path, HttpMethod method)
        {
            var token = await _tokenProvider.RetrieveAsync();
            var httpRequestMessage = new HttpRequestMessage(method, path)
            {
                Headers =
                {
                    { HttpRequestHeader.Authorization.ToString(), $"Bearer {token}" },
                    { HttpRequestHeader.Accept.ToString(), _accept }
                }
            };

            return httpRequestMessage;
        }

        public async Task<TResponse> PostAsync<TRequest, TResponse>(string path, TRequest body)
        {
            return await PostAsync<TRequest, TResponse>(path, body, () => Enumerable.Empty<(string, string)>());
        }

        public async Task<TResponse> PostAsync<TRequest, TResponse>(string path, TRequest body, Func<IEnumerable<(string, string)>> headerFactory)
        {
            var requestMessage = await PrepareRequest(path, HttpMethod.Post);

            // add any additional headers using the supplied callback
            headerFactory.Invoke().ToList().ForEach(h => requestMessage.Headers.Add(h.Item1, h.Item2));

            var json = JsonConvert.SerializeObject(body);

            requestMessage.Content = new StringContent(json);

            var response = await Client().SendAsync(requestMessage);

            response.EnsureSuccessStatusCode();

            var responseContentJson = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<TResponse>(responseContentJson);
        }

        public async Task<(TResponse SuccessResponse, string FailResponse)> PatchAsync<TRequest, TResponse>(string path, TRequest body, Func<IEnumerable<(string, string)>> headerFactory)
        {
            var requestMessage = await PrepareRequest(path, HttpMethod.Patch);

            // add any additional headers using the supplied callback
            headerFactory.Invoke().ToList().ForEach(h => requestMessage.Headers.Add(h.Item1, h.Item2));

            var json = JsonConvert.SerializeObject(body);

            requestMessage.Content = new StringContent(json);

            var response = await Client().SendAsync(requestMessage);

            var responseContentJson = await response.Content.ReadAsStringAsync();

            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException)
            {
                // If the response content is empty, at least make a new ApiHttpError with the status code.
                if (string.IsNullOrEmpty(responseContentJson))
                {
                    responseContentJson = JsonConvert.SerializeObject(new ApiHttpError() { Status = response.StatusCode.ToString() });
                }
                return (default, responseContentJson);
            }

            return (JsonConvert.DeserializeObject<TResponse>(responseContentJson), default);
        }

        public async Task<TResponse> GetAsync<TResponse, TRequest>(string path, TRequest requestObject) where TRequest : class, new()
        {
            return await GetAsync<TResponse>(path, QueryStringBuilder.ToQueryString(requestObject));
        }

        public async Task<TResponse> GetAsync<TResponse>(string path, string query = null)
        {
            if (!string.IsNullOrEmpty(query))
            {
                path += query;
            }
            var requestMessage = await PrepareRequest(path, HttpMethod.Get);
            var response = await Client().SendAsync(requestMessage);

            response.EnsureSuccessStatusCode();

            var responseContentJson = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<TResponse>(responseContentJson);
        }

        public async Task<(TResponse Response, int StatusCode)> TryGetAsync<TResponse>(string path, IEnumerable<(string, string)> headerFactory = null, string query = null)
        {
            if (!string.IsNullOrEmpty(query))
            {
                path = $"{path}?{query}";
            }
            var requestMessage = await PrepareRequest(path, HttpMethod.Get);

            // add any additional headers using the supplied callback
            headerFactory?.ToList().ForEach(h => requestMessage.Headers.Add(h.Item1, h.Item2));

            var response = await Client().SendAsync(requestMessage);

            try
            {
                response.EnsureSuccessStatusCode();

                var responseContentJson = await response.Content.ReadAsStringAsync();

                return (JsonConvert.DeserializeObject<TResponse>(responseContentJson), (int)response.StatusCode);
            }
            catch (HttpRequestException)
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return (default, (int)response.StatusCode);
                }
                throw;
            }
        }

        private HttpClient Client()
        {
            var clientName = typeof(T).Name;
            return _clientFactory.CreateClient(clientName);
        }
    }
}
