using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using System.Linq;

namespace TaxCalculator.Rest
{
    internal class RestHttpClient : IHttpRestClient
    {
        private static readonly HttpClient httpClient = new HttpClient();


        private HttpRequestMessage ConstructRequest(string uri, HttpMethod method, IEnumerable<KeyValuePair<string, string>>? headers = null)
        {
            HttpRequestMessage request = new HttpRequestMessage(method, uri);

            if (headers != null)
            {
                foreach(KeyValuePair<string, string> keyValue in headers)
                {
                    request.Headers.Add(keyValue.Key, keyValue.Value);
                }
            }

            return request;
        }

        private string AddQueryParameters(string uri, IEnumerable<KeyValuePair<string, string>>? parameters)
        {
            if (parameters != null)
            {
                UriBuilder uriBuilder = new UriBuilder(uri)
                {
                    Query = string.Join("&", parameters.Select(keyValue => $"{Uri.EscapeDataString(keyValue.Key)}={Uri.EscapeDataString(keyValue.Value)}"))
                };
                return uriBuilder.Uri.ToString();
            }
            return uri;
        }

        private async Task<IHttpRestResponse> PerformRequest(HttpRequestMessage request)
        {
            try
            {
                return new RestHttpClientResponse(await httpClient.SendAsync(request).ConfigureAwait(false));
            }
            catch (HttpRequestException ex)
            {
                throw new RequestConnectivityException("Could not complete request. Unable to reach destination.", ex);
            }
            catch (TaskCanceledException ex)
            {
                throw new RequestTimeoutException("Request timed out", ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new RequestSendFailedException("The request URI is invalid", ex);
            }
        }

        public async Task<IHttpRestResponse> JsonPostJsonResponse(string uri, string requestBodyJson, IEnumerable<KeyValuePair<string, string>>? headers = null)
        {
            headers = headers.Append(new KeyValuePair<string, string>("Content-Type", "application/json"));
            HttpRequestMessage request = ConstructRequest(uri, HttpMethod.Post, headers);
            request.Content = new StringContent(requestBodyJson);
            return await PerformRequest(request).ConfigureAwait(false);
        }
        public async Task<IHttpRestResponse> GetJsonResponse(string uri, IEnumerable<KeyValuePair<string, string>>? parameters = null, IEnumerable<KeyValuePair<string, string>>? headers = null)
        {
            uri = AddQueryParameters(uri, parameters);
            HttpRequestMessage request = ConstructRequest(uri, HttpMethod.Get, headers);
            return await PerformRequest(request).ConfigureAwait(false);
        }
    }
}
