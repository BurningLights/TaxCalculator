using System.Net.Http;
using System.Threading.Tasks;

namespace TaxCalculator.Rest
{
    internal class RestHttpClientResponse : IHttpRestResponse
    {
        public int StatusCode => (int)httpResponse.StatusCode;

        public bool IsSuccess => httpResponse.IsSuccessStatusCode;

        public string CodeReason => httpResponse.ReasonPhrase;

        private readonly HttpResponseMessage httpResponse;

        public RestHttpClientResponse(HttpResponseMessage httpResponse) => this.httpResponse = httpResponse;

        public async Task<string> GetBodyAsync() => await httpResponse.Content.ReadAsStringAsync();
    }
}
