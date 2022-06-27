using System.Collections.Generic;
using TaxCalculator.Data;
using TaxCalculator.Services.TaxCalculators.TaxJar.Requests;
using TaxCalculator.Services.TaxCalculators.TaxJar.Responses;
using System.Linq;
using System.Threading.Tasks;
using TaxCalculator.Json;
using TaxCalculator.Rest;

namespace TaxCalculator.Services.TaxCalculators.TaxJar
{
    internal class TaxJarCalculator : ITaxCalculator
    {
        const string API_ENDPOINT = "https://api.taxjar.com/v2/";
        const string TAXES_PATH = "taxes";
        const string RATES_PATH = "rates";

        private readonly string apiKey;
        public string ApiVersion { get; private set; }

        private readonly IHttpRestClient restClient;
        private readonly IJsonConverter jsonConverter;

        #region Countries
        public const string UNITED_STATES = "US";
        public const string CANADA = "CA";
        public const string AUSTRALIA = "AU";
        public const string AUSTRIA = "AT";
        public const string BELGIUM = "BE";
        public const string BULGARIA = "BG";
        public const string CROATIA = "HR";
        public const string CYPRUS = "CY";
        public const string CZECH_REPUBLIC = "CZ";
        public const string DENMARK = "DK";
        public const string ESTONIA = "EE";
        public const string FINLAND = "FI";
        public const string FRANCE = "FR";
        public const string GERMANY = "DE";
        public const string GREECE = "GR";
        public const string HUNGARY = "HU";
        public const string IRELAND = "IE";
        public const string ITALY = "IT";
        public const string LATVIA = "LV";
        public const string LITHUANIA = "LT";
        public const string LUXEMBOURG = "LU";
        public const string MALTA = "MT";
        public const string NETHERLANDS = "NL";
        public const string POLAND = "PL";
        public const string PORTUGAL = "PT";
        public const string ROMANIA = "RO";
        public const string SLOVAKIA = "SK";
        public const string SLOVENIA = "SI";
        public const string SPAIN = "ES";
        public const string SWEDEN = "SE";
        public const string UNITED_KINGDOM = "GB";

        private static readonly string[] EU_CONTRIES = new string[]
        {
            AUSTRIA, BELGIUM, BULGARIA, CROATIA, CYPRUS, CZECH_REPUBLIC, DENMARK, ESTONIA, FINLAND, FRANCE, GERMANY, GREECE,
            HUNGARY, IRELAND, ITALY, LATVIA, LITHUANIA, LUXEMBOURG, MALTA, NETHERLANDS, POLAND, PORTUGAL, ROMANIA,
            SLOVAKIA, SLOVENIA, SPAIN, SWEDEN, UNITED_KINGDOM
        };

        private static readonly string[] ALL_COUNTRIES = new string[]
        {
            UNITED_STATES, CANADA, AUSTRALIA
        }.Concat(EU_CONTRIES).ToArray();
        
        #endregion

        public TaxJarCalculator(IHttpRestClient restClient, IJsonConverter jsonConverter, string apiKey, string apiVersion = null)
        {
            this.apiKey = apiKey;
            ApiVersion = apiVersion;
            this.restClient = restClient;
            this.jsonConverter = jsonConverter;
        }

        private IDictionary<string, string> GetHeaders()
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

        private IDictionary<string, string> RatesRequestToParameterDict(RatesRequest request)
        {
            try
            {
                return jsonConverter.DeserializeObject<Dictionary<string, string>>(jsonConverter.SerializeObject(request));
            }
            catch (JsonConversionException ex)
            {
                throw new ServiceInternalException("Error converting RatesRequest to parameter dictionary", ex);
            }
        }

        private async Task RaiseResponseExceptions(IHttpRestResponse response)
        {
            if (!response.IsSuccess)
            {
                if (response.StatusCode == 401)
                {
                    throw new ServiceConfigurationException("API key is unauthorized");
                }
                else if (response.StatusCode == 403)
                {
                    throw new ServiceConfigurationException("The API key is not authorized for this use");
                }
                else
                {
                    try
                    {
                        ErrorResponse detail = jsonConverter.DeserializeObject<ErrorResponse>(await response.GetBodyAsync().ConfigureAwait(false));
                        throw new ServiceInternalException($"Request failed with error: {detail.ErrorName} - {detail.Detail}");
                    }
                    catch (DeserializationException)
                    {
                        throw new ServiceInternalException($"Request failed with error: {response.StatusCode} - {response.CodeReason}");
                    }
                }
            }
        }

        private async Task<T> DecodeResponseBody<T>(IHttpRestResponse response)
        {
            try
            {
                return jsonConverter.DeserializeObject<T>(await response.GetBodyAsync().ConfigureAwait(false));
            }
            catch (DeserializationException ex)
            {
                throw new ServiceInternalException("Could not decode API response", ex);
            }
        }


        public async Task<decimal> GetTaxRate(IAddress address)
        {
            if (string.IsNullOrEmpty(address?.Zip))
            {
                throw new ServiceInputException("The address Zip is required");
            }
            if (!string.IsNullOrEmpty(address.Country) && !IsValidCountry(address.Country))
            {
                throw new ServiceInputException($"The address Country {address.Country} is not a supported country");
            }
            if (!string.IsNullOrEmpty(address.State) && address.State.Length != 2)
            {
                throw new ServiceInputException($"The address State {address.State} is invalid. It must be a 2-letter state code");
            }

            IDictionary<string, string> parameters = RatesRequestToParameterDict(new RatesRequest(address));

            IHttpRestResponse response;
            try
            {
                response = await restClient.GetJsonResponse(ConstructUri(RATES_PATH, address.Zip), parameters, GetHeaders()).ConfigureAwait(false);
            }
            catch (RequestException ex)
            {
                throw new ServiceInternalException("Could not complete Get Tax Rate request", ex);
            }
            await RaiseResponseExceptions(response).ConfigureAwait(false);
            RatesResponseWrapper decodedResponse = await DecodeResponseBody<RatesResponseWrapper>(response).ConfigureAwait(false);

            if (IsCountryEu(decodedResponse.Rate.Country))
            {
                return decodedResponse.Rate.StandardRate;
            }
            else if (IsValidCountry(decodedResponse.Rate.County))
            {
                return decodedResponse.Rate.TotalTaxRate;
            }
            else
            {
                throw new ServiceInternalException($"The API returned an unknown country code of {decodedResponse.Rate.Country}");
            }
        }

        public async Task<decimal> CalculateTaxes(IAddress fromAddress, IAddress toAddress, decimal amount, decimal shipping)
        {
            if (string.IsNullOrEmpty(toAddress?.Country))
            {
                throw new ServiceInputException("The To Address Country is required"); 
            }

            string requestBody;
            try
            {
                requestBody = jsonConverter.SerializeObject(new TaxesRequest(fromAddress, toAddress, amount, shipping));
            }
            catch (SerializationException ex)
            {
                throw new ServiceInternalException("Could not serialize fromAddress, toAddress, amount, and shipping into valid request", ex);
            }

            IHttpRestResponse response;
            try
            {
                response = await restClient.JsonPostJsonResponse(ConstructUri(TAXES_PATH), requestBody, GetHeaders()).ConfigureAwait(false);
            }
            catch (RequestException ex)
            {
                throw new ServiceInternalException("Could not complete Calculate Taxes request", ex);
            }
            await RaiseResponseExceptions(response).ConfigureAwait(false);

            TaxesResponseWrapper decodedResponse = await DecodeResponseBody<TaxesResponseWrapper>(response).ConfigureAwait(false);

            return decodedResponse.Tax.TaxToCollect;
        }

        public bool IsCountryEu(string country) => EU_CONTRIES.Contains(country);

        public bool IsValidCountry(string country) => ALL_COUNTRIES.Contains(country);

        public IEnumerable<string> SupportedCountries() => ALL_COUNTRIES;
    }
}
