using System;
using System.Collections.Generic;
using TaxCalculator.Data;
using TaxCalculator.Services.Http;
using TaxCalculator.Services.TaxCalculators.TaxJar.Requests;
using TaxCalculator.Services.TaxCalculators.TaxJar.Responses;

namespace TaxCalculator.Services.TaxCalculators.TaxJar
{
    internal class TaxJarCalculator : ITaxCalculator
    {
        const string API_ENDPOINT = "https://api.taxjar.com/v2/";
        const string TAXES_PATH = "taxes";
        const string RATES_PATH = "rates";

        private readonly string apiKey;
        public string ApiVersion { get; private set; }

        private readonly IRestClient restClient;

        public TaxJarCalculator(IRestClient restClient, string apiKey, string apiVersion = null)
        {
            this.apiKey = apiKey;
            ApiVersion = apiVersion;
            this.restClient = restClient;
        }

        private IDictionary<string, string> getHeaders()
        {
            IDictionary<string, string> headers = new Dictionary<string, string>
            {
                { "Authorization", $"Bearer {apiKey}" }
            };
            if (!string.IsNullOrEmpty(ApiVersion))
            {
                headers.Add("x-api-version", ApiVersion);
            }

            return headers;
        }

        private string ConstructUri(string fragment, string resourceParam = null)
        {
            string uri = API_ENDPOINT + fragment;
            return !string.IsNullOrEmpty(resourceParam) ? uri + $"/{resourceParam}" : uri;
        }

        public decimal GetTaxRate(IAddress address)
        {
            // TODO: Address validation

            // TODO: Exception handling
            RatesResponseWrapper taxRates = restClient.JsonPostRequestResponse<RatesResponseWrapper>(
                ConstructUri(RATES_PATH, address.Zip), new RatesRequest(address), getHeaders()
            );

            // TODO: Determine which value to return
        }
        public decimal CalculateTaxes(IAddress fromAddress, IAddress toAddress, decimal amount, decimal shipping)
        {

            // TODO: Address validation

            // TODO: Exception handling

            TaxesResponseWrapper taxes = restClient.JsonPostRequestResponse<TaxesResponseWrapper>(
                ConstructUri(TAXES_PATH), new TaxesRequest(toAddress, fromAddress, shipping, amount), getHeaders()
            );

            return taxes.Tax.TaxToCollect;
        }
        public IList<string> SupportedCountries() => throw new NotImplementedException();
    }
}
