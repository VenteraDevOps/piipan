using System.Collections.Generic;
using Piipan.Match.Api.Models;

namespace Piipan.QueryTool.Client.Models
{
    public class QueryFormModel
    {
        public PiiQuery Query { get; set; } = new();

        /// <summary>
        /// The search response that is collected after the user hits "Search" and an API call is made
        /// </summary>
        public OrchMatchResponseData QueryResult { get; set; }
        public string Token { get; set; }
        public List<ServerError> ServerErrors { get; set; } = new();
        public string Location { get; set; }
    }
}
