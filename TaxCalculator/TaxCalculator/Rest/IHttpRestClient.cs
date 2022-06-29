using System.Collections.Generic;
using System.Threading.Tasks;

namespace TaxCalculator.Rest
{
    public interface IHttpRestClient
    {
        Task<IHttpRestResponse> JsonPostJsonResponse(string uri, string requestBodyJson, IEnumerable<KeyValuePair<string, string>>? headers = null);
        Task<IHttpRestResponse> GetJsonResponse(string uri, IEnumerable<KeyValuePair<string, string>>? parameters = null, IEnumerable<KeyValuePair<string, string>>? headers = null);
    }
}
