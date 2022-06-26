using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Threading.Tasks;

namespace TaxCalculator.Services.Rest
{
    internal interface IHttpRestClient
    {
        Task<IHttpRestResponse> JsonPostJsonResponse(string uri, string requestBodyJson, IEnumerable<KeyValuePair<string, string>> headers = null);
        Task<IHttpRestResponse> GetJsonResponse(string uri, IEnumerable<KeyValuePair<string, string>> parameters = null, IEnumerable<KeyValuePair<string, string>> headers = null);
    }
}
