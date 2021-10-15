using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Piipan.Shared.Http
{
    public interface IAuthorizedApiClient<T>
    {
        /// <summary>
        /// Send an asynchronous POST request to an API endpoint
        /// </summary>
        /// <param name="uri">URI of the API endpoint</param>
        /// <param name="body">Request body</param>
        Task<TResponse> PostAsync<TRequest, TResponse>(string path, TRequest body);

        /// <summary>
        /// Send an asynchronous GET request to an API endpoint
        /// </summary>
        /// <param name="uri">URI of the API endpoint</param>
        Task<TResponse> GetAsync<TResponse>(string path);
    }
}