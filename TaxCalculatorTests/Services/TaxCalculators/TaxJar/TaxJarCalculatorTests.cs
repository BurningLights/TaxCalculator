using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaxCalculator.Services.TaxCalculators.TaxJar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaxCalculator.Rest;
using Moq;
using TaxCalculator.Json;
using TaxCalculator.Data;
using TaxCalculator.Services.TaxCalculators.TaxJar.Responses;

namespace TaxCalculator.Services.TaxCalculators.TaxJar.Tests
{
    [TestClass()]
    public class TaxJarCalculatorTests
    {
        #region TestData
        private static readonly string[] expectedEuCountries = new string[]
        {
            "AT", "BE", "BG", "HR", "CY", "CZ", "DK", "EE", "FI", "FR", "DE", "GR", "HU", "IE",
            "IT", "LV", "LT", "LU", "MT", "NL", "PL", "PT", "RO", "SK", "SI", "ES", "SE", "GB"
        };
        private static readonly string[] expectedNonEuCountries = new string[] { "US", "CA", "AU" };

        private static readonly string[] nonCountryCodes = new string[] { "A", "AB", "ABC" };
        private static readonly string[] unsupportedCountryCodes = new string[] { "BR", "EG", "CN" };

        private static readonly object[][] authErrorResults = new object[][]
        {
            new object[] {401, "Unauthorized", "Your API key is wrong."},
            new object[] {403, "Forbidden", "The resource requested is not authorized for use." }
        };
        private static readonly object[][] definedErrorResults = new object[][]
        {
            new object[] {400, "Bad Request", "Your request format is bad."},
            new object[] {404, "Not Found", "The specified resource could not be found." },
            new object[] {405, "Method Not Allowed", "You tried to access a resource with an invalid method."},
            new object[] {406, "Not Acceptable", "Your request is not acceptable." },
            new object[] {410, "Gone", "The resource requested has been removed from our servers."},
            new object[] {422, "Unprocessable Entity", "Your request could not be processed." },
            new object[] {429, "Too Many Requests", "Too Many Requests – You’re requesting too many resources! Slow down!"},
            new object[] {500, "Internal Server Error", "Internal Server Error – We had a problem with our server. Try again later." },
            new object[] {503, "Service Unavailable", "We’re temporarily offline for maintenance. Try again later." }
        };

        private const string ValidUsRateResponseBody = /*lang=json,strict*/ @"
            {
              ""rate"": {
                ""zip"": ""90404"",
                ""state"": ""CA"",
                ""state_rate"": ""0.0625"",
                ""county"": ""LOS ANGELES"",
                ""county_rate"": ""0.01"",
                ""city"": ""SANTA MONICA"",
                ""city_rate"": ""0.0"",
                ""combined_district_rate"": ""0.025"",
                ""combined_rate"": ""0.0975"",
                ""freight_taxable"": false
              }
            }";

        private const string ratesUri = "https://api.taxjar.com/v2/rates/{0}";
        private const string taxesUri = "https://api.taxjar.com/v2/taxes";
        private const string apiKey = "9e0cd62a22f451701f29c3bde214";
        private const string version = "2022-01-24";
        #endregion

        #region Helpers
        private static object[] StringToObjectArray(string input) => new object[] { input };

        private static IEnumerable<string> GetAllExpectedCountries() => expectedNonEuCountries.Concat(expectedEuCountries);
        private static IEnumerable<object[]> AllExpectedCountriesData() => GetAllExpectedCountries().Select(StringToObjectArray);
        private static IEnumerable<object[]> ExpectedEuCountriesData() => expectedEuCountries.Select(StringToObjectArray);
        private static IEnumerable<object[]> NonEuCountriesData() => expectedNonEuCountries.Select(StringToObjectArray);
        private static IEnumerable<object[]> InvalidCountryCodesDataIncludeEmpty() => nonCountryCodes.Append("").Select(StringToObjectArray);
        private static IEnumerable<object[]> InvalidCountryCodesData() => nonCountryCodes.Select(StringToObjectArray);
        private static IEnumerable<object[]> UnsupportedCountryCodesData() => unsupportedCountryCodes.Select(StringToObjectArray);

        private static IEnumerable<object[]> AuthErrorResults() => authErrorResults;
        private static IEnumerable<object[]> DefinedErrorResults() => definedErrorResults;

        private static TaxJarCalculator CalculatorWithStubs() 
        {
            IHttpRestClient restClientStub = Mock.Of<IHttpRestClient>();
            IJsonConverter jsonConverterStub = Mock.Of<IJsonConverter>();
            return new(restClientStub, jsonConverterStub, "");
        }

        private static string ErrorResponse(int statusCode, string errorMessage, string errorDetail) => $"{{\"error\": \"{errorMessage}\", \"detail\": \"{errorDetail}\", \"status\": \"{statusCode}\"}}";

        private static string UsRateResponseJson(string zip, string state, decimal stateRate, string county, decimal countyRate, string city, 
            decimal cityRate, decimal combinedDistrictRate, decimal combinedRate, bool freightTaxable)
        {
            return @$"{{""rate"": {{
""zip"": ""{zip}"",
""state"": ""{state}"",
""state_rate"": ""{stateRate}"",
""county"": ""{county}"",
""county_rate"": ""{countyRate}"",
""city"": ""{city}"",
""city_rate"": ""{cityRate}"",
""combined_district_rate"": ""{combinedDistrictRate}"",
""combined_rate"": ""{combinedRate}"",
""freight_taxable"": ""{freightTaxable}"",
}}}}";
        }

        private static string UsRateResponseWithCountry(string zip, string country, decimal countryRate, string state, decimal stateRate, string county, decimal countyRate, string city,
            decimal cityRate, decimal combinedDistrictRate, decimal combinedRate, bool freightTaxable)
        {
            return @$"{{""rate"": {{
""zip"": ""{zip}"",
""country"": ""{country}"",
""countryRate"": ""{countryRate}"",
""state"": ""{state}"",
""state_rate"": ""{stateRate}"",
""county"": ""{county}"",
""county_rate"": ""{countyRate}"",
""city"": ""{city}"",
""city_rate"": ""{cityRate}"",
""combined_district_rate"": ""{combinedDistrictRate}"",
""combined_rate"": ""{combinedRate}"",
""freight_taxable"": ""{freightTaxable}"",
}}}}";

        }

        private static string CaRateResponseJson(string zip, string city, string state, string country, decimal combinedRate, bool freightTaxable)
        {
            return @$"{{""rate"": {{
""zip"": ""{zip}"",
""city"": ""{city}"",
""state"": ""{state}"",
""country"": ""{country}"",
""combined_rate"": ""{combinedRate}"",
""freight_taxable"": ""{freightTaxable}"",
}}}}";
        }

        private static string AuRateResponseJson(string zip, string country, decimal countryRate, decimal combinedRate, bool freightTaxable)
        {
            return @$"{{""rate"": {{
""zip"": ""{zip}"",
""country"": ""{country}"",
""country_rate"": ""{countryRate}"",
""combined_rate"": ""{combinedRate}"",
""freight_taxable"": ""{freightTaxable}"",
}}}}";
        }

        private static string EuRateResponseJson(string country, string name, decimal standardRate, decimal reducedRate, decimal superReducedRate, decimal parkingRate, 
            decimal distanceSaleThreshold, bool freightTaxable)
        {
            return @$"{{""rate"": {{
""country"": ""{country}"",
""name"": ""{name}"",
""standard_rate"": ""{standardRate}"",
""reduced_rate"": ""{reducedRate}"",
""super_reduced_rate"": ""{superReducedRate}"",
""parking_rate"": ""{parkingRate}"",
""distance_sale_threshold"": ""{distanceSaleThreshold}"",
""freight_taxable"": ""{freightTaxable}"",
}}}}";
        }

        private static string TaxesRequestJson(string fromCountry, string fromZip, string? fromState, string fromCity, string fromStreet, 
            string toZip, string? toState, string toCity, string toStreet, decimal amount, decimal shipping)
        {
            StringBuilder states = new();
            if (fromState != null)
            {
                states.Append($",\n\"from_state\": \"{fromState}\"");
            }
            if (toState != null)
            {
                states.Append($",\n\"to_state\": \"{toState}\"");
            }

            return $@"{{
""from_country"": ""{fromCountry}"",
""from_zip"": ""{fromZip}"",
""from_city"": ""{fromCity}"",
""from_street"": ""{fromStreet}"",
""to_country"": ""{fromCountry}"",
""to_zip"": ""{toZip}"",
""to_city"": ""{toCity}"",
""to_street"": ""{toStreet}"",
""amount"": {amount},
""shipping"": {shipping}{states}
}}";
        }

        private static string TaxesResponseJson(decimal orderTotal, decimal shipping, decimal amountToCollect, bool hasNexus, bool freightTaxable, string? taxSource, 
            string country, string? state = null, string? county = null, string? city = null)
        {
            decimal taxableAmount = freightTaxable ? orderTotal + shipping : orderTotal;
            decimal rate = amountToCollect / taxableAmount;

            StringBuilder jurisdications = new($"{{\"country\": \"{country}\"");
            if (state != null)
            {
                jurisdications.Append($", \"state\": \"{state}\"");
            }
            if (county != null)
            {
                jurisdications.Append($", \"county\": \"{county}\"");
            }
            if (city != null)
            {
                jurisdications.Append($", \"city\": \"{city}\"");
            }
            jurisdications.Append("}");

            return @$"{{""tax"": {{
""order_total_amount"": {orderTotal},
""shipping"": {shipping},
""taxable_amount"": {taxableAmount},
""amount_to_collect"": {amountToCollect},
""rate"": {rate},
""hax_nexus"": {hasNexus},
""freight_taxable"": {freightTaxable},
""tax_source"": {taxSource ?? "null"},
""jurisdications"": {jurisdications}
}}}}";
        }

        private static IHttpRestResponse ValidResponse(string body) => Mock.Of<IHttpRestResponse>(
            response => response.IsSuccess == true && response.StatusCode == 200 && response.CodeReason == "OK" &&
            response.GetBodyAsync().Result == body
        );

        private static bool CheckParametersExpected(IEnumerable<KeyValuePair<string, string>> parameters, IDictionary<string, string> expected)
        {
            return parameters.Count() == expected.Count && parameters.Intersect(expected).Count() == expected.Count;
        }

        private static bool CheckHeadersExpected(IEnumerable<KeyValuePair<string, string>> parameters, string apiKey, string? version = null)
        {
            string tokenAuthHeader = $"Token token=\"{apiKey}\"";
            string bearerAuthHeader = $"Bearer {apiKey}";
            bool hasHeaders = parameters.Where(val => val.Key == "Authorization" && (val.Value == tokenAuthHeader || val.Value == bearerAuthHeader)).Any();
            if (version == null)
            {
                hasHeaders &= !parameters.Where(val => val.Key == "x-api-version").Any();
            }
            else
            {
                hasHeaders &= parameters.Where(val => val.Key == "x-api-version" && val.Value == version).Any();
            }

            return hasHeaders;
        }

        #endregion

        #region GetTaxRate
        [TestMethod()]
        [DataRow(null)]
        [DataRow("")]
        public async Task GetTaxRate_EmptyToZip_ThrowsServiceInputException(string? zip)
        {
           TaxJarCalculator calculator = CalculatorWithStubs();
            IAddress addressStub = Mock.Of<IAddress>(
                address => address.Country == "US" && address.City == "Test" && address.State == "MD" && address.Zip == zip && address.StreetAddress == "123 E Main St"
            );

            await Assert.ThrowsExceptionAsync<ServiceInputException>(() => calculator.GetTaxRate(addressStub));
        }

        [TestMethod()]
        public async Task GetTaxRate_UnsupportedToCountry_ThrowsServiceInputException()
        {
            TaxJarCalculator calculator = CalculatorWithStubs();
            IAddress addressStub = Mock.Of<IAddress>(
                address => address.Country == "BR" && address.City == "Test" && address.State == "MD" && address.Zip == "21045" && address.StreetAddress == "123 E Main St"
            );

            await Assert.ThrowsExceptionAsync<ServiceInputException>(() => calculator.GetTaxRate(addressStub));
        }

        [TestMethod()]
        [DynamicData(nameof(InvalidCountryCodesData), DynamicDataSourceType.Method)]
        public async Task GetTaxRate_InvalidToCountry_ThrowsServiceInputException(string countryCode)
        {
            TaxJarCalculator calculator = CalculatorWithStubs();
            IAddress addressStub = Mock.Of<IAddress>(
                address => address.Country == countryCode && address.City == "Test" && address.State == "MD" && address.Zip == "21045" && address.StreetAddress == "123 E Main St"
            );

            await Assert.ThrowsExceptionAsync<ServiceInputException>(() => calculator.GetTaxRate(addressStub));
        }


        [TestMethod()]
        public async Task GetTaxRate_JsonConverter_DeserializationError_ThrowsServiceInternalException()
        {
            Mock<IJsonConverter> jsonConverterMock = new();
            jsonConverterMock.Setup(json => json.DeserializeObject<object>(It.IsAny<string>())).Throws<DeserializationException>();

            IHttpRestResponse response = Mock.Of<IHttpRestResponse>(
                response => response.IsSuccess == true && response.StatusCode == 200 && response.CodeReason == "OK"
                && response.GetBodyAsync().Result == ValidUsRateResponseBody);

            Mock<IHttpRestClient> restClientMock = new();
            restClientMock.Setup(client => client.GetRequest(
                It.IsAny<string>(), It.IsAny<IEnumerable<KeyValuePair<string, string>>?>(), It.IsAny<IEnumerable<KeyValuePair<string, string>>?>()
            )).ReturnsAsync(response);

            IAddress addressStub = Mock.Of<IAddress>(
                address => address.Country == "US" && address.City == "Santa Monica" && address.State == "CA" && address.Zip == "90404" && address.StreetAddress == null
            );

            TaxJarCalculator calculator = new(restClientMock.Object, jsonConverterMock.Object, "");

            await Assert.ThrowsExceptionAsync<ServiceInternalException>(() => calculator.GetTaxRate(addressStub));
        }

        [TestMethod()]
        public async Task GetTaxRate_RestHttpClient_RequestException_ThrowsServiceInternalException()
        {
            Mock<IHttpRestClient> restClientMock = new();
            restClientMock.Setup(client => client.GetRequest(
                It.IsAny<string>(), It.IsAny<IEnumerable<KeyValuePair<string, string>>?>(), It.IsAny<IEnumerable<KeyValuePair<string, string>>?>()
            )).ThrowsAsync(new RequestException());

            IJsonConverter converterStub = Mock.Of<IJsonConverter>();

            IAddress addressStub = Mock.Of<IAddress>(
                address => address.Country == "US" && address.City == "Santa Monica" && address.State == "CA" && address.Zip == "90404" && address.StreetAddress == null
            );

            TaxJarCalculator calculator = new(restClientMock.Object, converterStub, "");


            await Assert.ThrowsExceptionAsync<ServiceInternalException>(() => calculator.GetTaxRate(addressStub));
        }

        [TestMethod()]
        [DynamicData(nameof(AuthErrorResults), DynamicDataSourceType.Method)]
        public async Task GetTaxRate_AuthUnsuccessfulResponse_ThrowsServiceConfigurationException(int errorCode, string errorMessage, string errorDetail)
        {
            string responseJson = ErrorResponse(errorCode, errorMessage, errorDetail);
            IHttpRestResponse response = Mock.Of<IHttpRestResponse>(
                response => response.IsSuccess == false && response.StatusCode == errorCode && response.CodeReason == errorDetail &&
                response.GetBodyAsync().Result == responseJson
            );

            Mock<IHttpRestClient> restClientMock = new();
            restClientMock.Setup(client => client.GetRequest(
                It.IsAny<string>(), It.IsAny<IEnumerable<KeyValuePair<string, string>>?>(), It.IsAny<IEnumerable<KeyValuePair<string, string>>?>()
            )).ReturnsAsync(response);

            ErrorResponse decodedResponse = new() { Detail = errorDetail, ErrorName = errorMessage, Status = errorCode.ToString()};

            IJsonConverter converterStub = Mock.Of<IJsonConverter>(converter => converter.DeserializeObject<ErrorResponse>(responseJson) == decodedResponse);

            IAddress addressStub = Mock.Of<IAddress>(
                address => address.Country == "US" && address.City == "Santa Monica" && address.State == "CA" && address.Zip == "90404" && address.StreetAddress == null
            );

            TaxJarCalculator calculator = new(restClientMock.Object, converterStub, "");


            await Assert.ThrowsExceptionAsync<ServiceConfigurationException>(() => calculator.GetTaxRate(addressStub));
        }

        [TestMethod()]
        [DynamicData(nameof(DefinedErrorResults), DynamicDataSourceType.Method)]
        public async Task GetTaxRate_ErrorResponse_ThrowsServiceInternalException(int errorCode, string errorMessage, string errorDetail)
        {
            string responseJson = ErrorResponse(errorCode, errorMessage, errorDetail);
            IHttpRestResponse response = Mock.Of<IHttpRestResponse>(
                response => response.IsSuccess == false && response.StatusCode == errorCode && response.CodeReason == errorDetail &&
                response.GetBodyAsync().Result == responseJson
            );

            Mock<IHttpRestClient> restClientMock = new();
            restClientMock.Setup(client => client.GetRequest(
                It.IsAny<string>(), It.IsAny<IEnumerable<KeyValuePair<string, string>>?>(), It.IsAny<IEnumerable<KeyValuePair<string, string>>?>()
            )).ReturnsAsync(response);

            ErrorResponse decodedResponse = new() { Detail = errorDetail, ErrorName = errorMessage, Status = errorCode.ToString() };

            IJsonConverter converterStub = Mock.Of<IJsonConverter>(converter => converter.DeserializeObject<ErrorResponse>(responseJson) == decodedResponse);

            IAddress addressStub = Mock.Of<IAddress>(
                address => address.Country == "US" && address.City == "Santa Monica" && address.State == "CA" && address.Zip == "90404" && address.StreetAddress == null
            );

            TaxJarCalculator calculator = new(restClientMock.Object, converterStub, "");

            await Assert.ThrowsExceptionAsync<ServiceInternalException>(() => calculator.GetTaxRate(addressStub));
        }

        [TestMethod()]
        public async Task GetTaxRate_OtherInvalidResponse_ThrowsServiceInternalException()
        {
            IHttpRestResponse response = Mock.Of<IHttpRestResponse>(
                response => response.IsSuccess == false && response.StatusCode == 410 && response.CodeReason == "I'm a Teapot" &&
                response.GetBodyAsync().Result == ""
            );

            Mock<IHttpRestClient> restClientMock = new();
            restClientMock.Setup(client => client.GetRequest(
                It.IsAny<string>(), It.IsAny<IEnumerable<KeyValuePair<string, string>>?>(), It.IsAny<IEnumerable<KeyValuePair<string, string>>?>()
            )).ReturnsAsync(response);

            Mock<IJsonConverter> converterMock = new();
            converterMock.Setup(converter => converter.DeserializeObject<ErrorResponse>("")).Throws<DeserializationException>();

            IAddress addressStub = Mock.Of<IAddress>(
                address => address.Country == "US" && address.City == "Santa Monica" && address.State == "CA" && address.Zip == "90404" && address.StreetAddress == null
            );

            TaxJarCalculator calculator = new(restClientMock.Object, converterMock.Object, "");


            await Assert.ThrowsExceptionAsync<ServiceInternalException>(() => calculator.GetTaxRate(addressStub));
        }

        [TestMethod()]
        [DataRow("")]
        [DataRow(null)]
        public async Task GetTaxRate_ZipOnly_UsTaxDecimal(string? otherElements)
        {
            string zip = "90404";
            string responseBody = UsRateResponseJson(zip, "CA", 0.0625m, "LOS ANGELES", 0.01m, "SANTA MONICA", 0, 0.025m, 0.0975m, false);
            RatesResponseWrapper ratesResponse = new()
            {
                Rate = new()
                {
                    Zip = zip,
                    State = "CA",
                    StateRate = 0.0625m,
                    County = "LOS ANGELES",
                    CountyRate = 0.01m,
                    City = "SANTA MONICA",
                    CityRate = 0,
                    TotalDistrictRate = 0.025m,
                    TotalTaxRate = 0.0975m,
                    FreightTaxable = false
                }
            };
            IHttpRestResponse response = ValidResponse(responseBody);

            Mock<IHttpRestClient> restClientMock = new();
            restClientMock.Setup(client => client.GetRequest(
                string.Format(ratesUri, zip), It.Is<IEnumerable<KeyValuePair<string, string>>?>(val => val == null || !val.Any()),
                It.Is<IEnumerable<KeyValuePair<string, string>>>(val => CheckHeadersExpected(val, apiKey, null))
            )).ReturnsAsync(response);

            IJsonConverter jsonConverter = Mock.Of<IJsonConverter>(converter => converter.DeserializeObject<RatesResponseWrapper>(responseBody) == ratesResponse);

            IAddress addressStub = Mock.Of<IAddress>(
                address => address.Country == otherElements && address.City == otherElements && address.State == otherElements && address.Zip == zip && address.StreetAddress == otherElements
            );

            TaxJarCalculator calculator = new(restClientMock.Object, jsonConverter, apiKey);


            Assert.AreEqual(0.0975m, await calculator.GetTaxRate(addressStub));
        }

        [TestMethod()]
        [DataRow(null)]
        [DataRow("2022-01-24")]
        public async Task GetTaxRate_UsAddress_CorrectDecimal(string? version)
        {
            string zip = "05495-2086";
            string country = "US";
            string state = "VT";
            string city = "Williston";
            string street = "312 Hurricane Lane";
            string responseBody = UsRateResponseWithCountry(zip, country, 0, state, 0.06m, "CHITTENDEN", 0, city.ToUpper(), 0, 0.01m, 0.07m, true);
            RatesResponseWrapper ratesResponse = new()
            {
                Rate = new()
                {
                    Zip = zip,
                    Country = country,
                    CountryRate = 0,
                    State = state,
                    StateRate = 0.06m,
                    County = "CHITTENDEN",
                    CountyRate = 0,
                    City = city.ToUpper(),
                    CityRate = 0,
                    TotalDistrictRate = 0.01m,
                    TotalTaxRate = 0.07m,
                    FreightTaxable = true
                }
            };
            IHttpRestResponse response = ValidResponse(responseBody);

            Dictionary<string, string> parameters = new()
            {
                {"country", country},
                {"state", state},
                {"city", city},
                {"street", street}
            };
            Mock<IHttpRestClient> restClientMock = new();
            restClientMock.Setup(client => client.GetRequest(
                string.Format(ratesUri, zip), It.Is<IEnumerable<KeyValuePair<string, string>>>(val => CheckParametersExpected(val, parameters)),
                It.Is<IEnumerable<KeyValuePair<string, string>>>(val => CheckHeadersExpected(val, apiKey, version))
            )).ReturnsAsync(response);

            IJsonConverter jsonConverter = Mock.Of<IJsonConverter>(converter => converter.DeserializeObject<RatesResponseWrapper>(responseBody) == ratesResponse);

            IAddress addressStub = Mock.Of<IAddress>(
                address => address.Country == country && address.City == city && address.State == state && address.Zip == zip && address.StreetAddress == street
            );

            TaxJarCalculator calculator = new(restClientMock.Object, jsonConverter, apiKey, version);

            Assert.AreEqual(0.07m, await calculator.GetTaxRate(addressStub));
        }

        [TestMethod()]
        public async Task GetTaxRate_CaAddress_CorrectDecimal()
        {
            string zip = "M5V1J1";
            string country = "CA";
            string state = "ON";
            string city = "Toronto";
            string street = "1 Blue Jays Way";
            string responseBody = CaRateResponseJson(zip, city, state, country, 0.13m, true);
            RatesResponseWrapper ratesResponse = new()
            {
                Rate = new()
                {
                    Zip = zip,
                    Country = country,
                    State = state,
                    City = city,
                    TotalTaxRate = 0.13m,
                    FreightTaxable = true
                }
            };
            IHttpRestResponse response = ValidResponse(responseBody);

            Dictionary<string, string> parameters = new()
            {
                {"country", country},
                {"state", state},
                {"city", city},
                {"street", street}
            };
            Mock<IHttpRestClient> restClientMock = new();
            restClientMock.Setup(client => client.GetRequest(
                string.Format(ratesUri, zip), It.Is<IEnumerable<KeyValuePair<string, string>>>(val => CheckParametersExpected(val, parameters)),
                It.Is<IEnumerable<KeyValuePair<string, string>>>(val => CheckHeadersExpected(val, apiKey, null))
            )).ReturnsAsync(response);

            IJsonConverter jsonConverter = Mock.Of<IJsonConverter>(converter => converter.DeserializeObject<RatesResponseWrapper>(responseBody) == ratesResponse);

            IAddress addressStub = Mock.Of<IAddress>(
                address => address.Country == country && address.City == city && address.State == state && address.Zip == zip && address.StreetAddress == street
            );

            TaxJarCalculator calculator = new(restClientMock.Object, jsonConverter, apiKey);

            Assert.AreEqual(0.13m, await calculator.GetTaxRate(addressStub));
        }

        [TestMethod()]
        public async Task GetTaxRate_AuAddress_CorrectDecimal()
        {
            string zip = "2000";
            string country = "AU";
            string state = "NSW";
            string city = "Sydney";
            string street = "2 Macquarie Street";
            string responseBody = AuRateResponseJson(zip, country, 0.1m, 0.1m, true);
            RatesResponseWrapper ratesResponse = new()
            {
                Rate = new()
                {
                    Zip = zip,
                    Country = country,
                    CountryRate = 0.1m,
                    TotalTaxRate = 0.1m,
                    FreightTaxable = true
                }
            };
            IHttpRestResponse response = ValidResponse(responseBody);

            Dictionary<string, string> parameters = new()
            {
                {"country", country},
                {"state", state},
                {"city", city},
                {"street", street}
            };
            Mock<IHttpRestClient> restClientMock = new();
            restClientMock.Setup(client => client.GetRequest(
                string.Format(ratesUri, zip), It.Is<IEnumerable<KeyValuePair<string, string>>>(val => CheckParametersExpected(val, parameters)),
                It.Is<IEnumerable<KeyValuePair<string, string>>>(val => CheckHeadersExpected(val, apiKey, null))
            )).ReturnsAsync(response);

            IJsonConverter jsonConverter = Mock.Of<IJsonConverter>(converter => converter.DeserializeObject<RatesResponseWrapper>(responseBody) == ratesResponse);

            IAddress addressStub = Mock.Of<IAddress>(
                address => address.Country == country && address.City == city && address.State == state && address.Zip == zip && address.StreetAddress == street
            );

            TaxJarCalculator calculator = new(restClientMock.Object, jsonConverter, apiKey);

            Assert.AreEqual(0.1m, await calculator.GetTaxRate(addressStub));
        }

        [TestMethod()]
        public async Task GetTaxRate_EuAddress_CorrectDecimal()
        {
            string zip = "75015";
            string country = "FR";
            string city = "Paris";
            string street = "156 Boulevard de Grenelle";
            string responseBody = EuRateResponseJson(country, "France", 0.2m, 0.1m, 0.055m, 0, 0, true);
            RatesResponseWrapper ratesResponse = new()
            {
                Rate = new()
                {
                    Country = country,
                    CountryName = "France",
                    StandardRate = 0.2m,
                    ReducedRate = 0.1m,
                    SuperReducedRate = 0.055m,
                    FreightTaxable = true
                }
            };
            IHttpRestResponse response = ValidResponse(responseBody);

            Dictionary<string, string> parameters = new()
            {
                {"country", country},
                {"city", city},
                {"street", street}
            };
            Mock<IHttpRestClient> restClientMock = new();
            restClientMock.Setup(client => client.GetRequest(
                string.Format(ratesUri, zip), It.Is<IEnumerable<KeyValuePair<string, string>>>(val => CheckParametersExpected(val, parameters)),
                It.Is<IEnumerable<KeyValuePair<string, string>>>(val => CheckHeadersExpected(val, apiKey, null))
            )).ReturnsAsync(response);

            IJsonConverter jsonConverter = Mock.Of<IJsonConverter>(converter => converter.DeserializeObject<RatesResponseWrapper>(responseBody) == ratesResponse);

            IAddress addressStub = Mock.Of<IAddress>(
                address => address.Country == country && address.City == city && address.State == null && address.Zip == zip && address.StreetAddress == street
            );

            TaxJarCalculator calculator = new(restClientMock.Object, jsonConverter, apiKey);

            Assert.AreEqual(0.2m, await calculator.GetTaxRate(addressStub));
        }
        #endregion

        #region CalculateTaxes
        [TestMethod()]
        [DataRow(null)]
        [DataRow("")]
        public async Task CalculateTaxes_EmptyToCountry_ThrowsServiceInputException(string? toCountry)
        {
            TaxJarCalculator calculator = CalculatorWithStubs();
            IAddress fromAddressStub = Mock.Of<IAddress>(
                address => address.Country == "US" && address.City == "Test" && address.State == "MD" && address.Zip == "21045" && address.StreetAddress == "123 E Main St"
            );
            IAddress toAddressStub = Mock.Of<IAddress>(
                address => address.Country == toCountry && address.City == "Test" && address.State == "MD" && address.Zip == "21045" && address.StreetAddress == "123 E Main St"
            );

            await Assert.ThrowsExceptionAsync<ServiceInputException>(() => calculator.CalculateTaxes(fromAddressStub, toAddressStub, 100m, 5m));
        }


        [TestMethod()]
        [DataRow(null)]
        [DataRow("")]
        public async Task CalculateTaxes_ToUsNoZip_ThrowsServiceInputExcption(string? toZip)
        {
            TaxJarCalculator calculator = CalculatorWithStubs();
            IAddress fromAddressStub = Mock.Of<IAddress>(
                address => address.Country == "US" && address.City == "Test" && address.State == "MD" && address.Zip == "21045" && address.StreetAddress == "123 E Main St"
            );
            IAddress toAddressStub = Mock.Of<IAddress>(
                address => address.Country == "US" && address.City == "Test" && address.State == "MD" && address.Zip == toZip && address.StreetAddress == "123 E Main St"
            );

            await Assert.ThrowsExceptionAsync<ServiceInputException>(() => calculator.CalculateTaxes(fromAddressStub, toAddressStub, 100m, 5m));
        }

        [TestMethod()]
        [DataRow("US", null)]
        [DataRow("US", "")]
        [DataRow("CA", null)]
        [DataRow("CA", "")]
        public async Task CalculateTaxes_ToUsOrCaNoState_ThrowsServiceInputException(string country, string? toState)
        {
            TaxJarCalculator calculator = CalculatorWithStubs();
            IAddress fromAddressStub = Mock.Of<IAddress>(
                address => address.Country == "US" && address.City == "Test" && address.State == "MD" && address.Zip == "21045" && address.StreetAddress == "123 E Main St"
            );
            IAddress toAddressStub = Mock.Of<IAddress>(
                address => address.Country == country && address.City == "Test" && address.State == toState && address.Zip == "12345" && address.StreetAddress == "123 E Main St"
            );

            await Assert.ThrowsExceptionAsync<ServiceInputException>(() => calculator.CalculateTaxes(fromAddressStub, toAddressStub, 100m, 5m));
        }

        [TestMethod()]
        [DynamicData(nameof(UnsupportedCountryCodesData), DynamicDataSourceType.Method)]
        public async Task CalculateTaxes_ToUnsupportedCountry_ThrowsServiceInputException(string countryCode)
        {
            TaxJarCalculator calculator = CalculatorWithStubs();
            IAddress fromAddressStub = Mock.Of<IAddress>(
                address => address.Country == "US" && address.City == "Test" && address.State == "MD" && address.Zip == "21045" && address.StreetAddress == "123 E Main St"
            );
            IAddress toAddressStub = Mock.Of<IAddress>(
                address => address.Country == countryCode && address.City == "Test" && address.State == "MD" && address.Zip == "12345" && address.StreetAddress == "123 E Main St"
            );

            await Assert.ThrowsExceptionAsync<ServiceInputException>(() => calculator.CalculateTaxes(fromAddressStub, toAddressStub, 100m, 5m));
        }

        [TestMethod()]
        [DynamicData(nameof(UnsupportedCountryCodesData), DynamicDataSourceType.Method)]
        public async Task CalculateTaxes_FromUnsupportedCountry_ThrowsServiceInputException(string countryCode)
        {
            TaxJarCalculator calculator = CalculatorWithStubs();
            IAddress fromAddressStub = Mock.Of<IAddress>(
                address => address.Country == countryCode && address.City == "Test" && address.State == "MD" && address.Zip == "21045" && address.StreetAddress == "123 E Main St"
            );
            IAddress toAddressStub = Mock.Of<IAddress>(
                address => address.Country == "US" && address.City == "Test" && address.State == "MD" && address.Zip == "12345" && address.StreetAddress == "123 E Main St"
            );

            await Assert.ThrowsExceptionAsync<ServiceInputException>(() => calculator.CalculateTaxes(fromAddressStub, toAddressStub, 100m, 5m));
        }

        [TestMethod()]
        [DynamicData(nameof(InvalidCountryCodesData), DynamicDataSourceType.Method)]
        public async Task CalculateTaxes_ToInvalidCountry_ThrowsServiceInputException(string countryCode)
        {
            TaxJarCalculator calculator = CalculatorWithStubs();
            IAddress fromAddressStub = Mock.Of<IAddress>(
                address => address.Country == "US" && address.City == "Test" && address.State == "MD" && address.Zip == "21045" && address.StreetAddress == "123 E Main St"
            );
            IAddress toAddressStub = Mock.Of<IAddress>(
                address => address.Country == countryCode && address.City == "Test" && address.State == "MD" && address.Zip == "12345" && address.StreetAddress == "123 E Main St"
            );

            await Assert.ThrowsExceptionAsync<ServiceInputException>(() => calculator.CalculateTaxes(fromAddressStub, toAddressStub, 100m, 5m));
        }

        [TestMethod()]
        [DynamicData(nameof(InvalidCountryCodesData), DynamicDataSourceType.Method)]
        public async Task CalculateTaxes_FromInvalidCountry_ThrowsServiceInputException(string countryCode)
        {
            TaxJarCalculator calculator = CalculatorWithStubs();
            IAddress fromAddressStub = Mock.Of<IAddress>(
                address => address.Country == countryCode && address.City == "Test" && address.State == "MD" && address.Zip == "21045" && address.StreetAddress == "123 E Main St"
            );
            IAddress toAddressStub = Mock.Of<IAddress>(
                address => address.Country == "US" && address.City == "Test" && address.State == "MD" && address.Zip == "12345" && address.StreetAddress == "123 E Main St"
            );

            await Assert.ThrowsExceptionAsync<ServiceInputException>(() => calculator.CalculateTaxes(fromAddressStub, toAddressStub, 100m, 5m));
        }

        [TestMethod()]
        [DataRow(-5.0)]
        [DataRow(0.0)]
        public async Task CalculateTaxes_AmountNotPositive_ThrowsServiceInputException(double amount)
        {
            TaxJarCalculator calculator = CalculatorWithStubs();
            IAddress fromAddressStub = Mock.Of<IAddress>(
                address => address.Country == "US" && address.City == "Test" && address.State == "MD" && address.Zip == "21045" && address.StreetAddress == "123 E Main St"
            );
            IAddress toAddressStub = Mock.Of<IAddress>(
                address => address.Country == "US" && address.City == "Test" && address.State == "MD" && address.Zip == "12345" && address.StreetAddress == "123 E Main St"
            );

            await Assert.ThrowsExceptionAsync<ServiceInputException>(() => calculator.CalculateTaxes(fromAddressStub, toAddressStub, Convert.ToDecimal(amount), 5m));
        }

        [TestMethod()]
        public async Task CalculateTaxes_JsonConverter_SerializationError_ThrowsServiceInternalException()
        {
            string fromCountry = "US";
            string fromCity = "La Jolla";
            string fromState = "CA";
            string fromZip = "92093";
            string fromStreet = "9500 Gilman Drive";
            string toCountry = "US";
            string toCity = "Los Angeles";
            string toState = "CA";
            string toZip = "90002";
            string toStreet = "1335 E 103rd St";
            decimal orderTotal = 16.5m;
            decimal shipping = 1.5m;

            IAddress fromAddressStub = Mock.Of<IAddress>(
                address => address.Country == fromCountry && address.City == fromCity && address.State == fromState && address.Zip == fromZip && address.StreetAddress == fromStreet
            );
            IAddress toAddressStub = Mock.Of<IAddress>(
                address => address.Country == toCountry && address.City == toCity && address.State == toState && address.Zip == toZip && address.StreetAddress == toStreet
            );

            Mock<IJsonConverter> jsonConverterMock = new();
            jsonConverterMock.Setup(json => json.SerializeObject(It.IsAny<object>())).Throws<SerializationException>();

            TaxJarCalculator calculator = new(Mock.Of<IHttpRestClient>(), jsonConverterMock.Object, "");

            await Assert.ThrowsExceptionAsync<ServiceInternalException>(() => calculator.CalculateTaxes(fromAddressStub, toAddressStub, orderTotal, shipping));
        }

        [TestMethod()]
        public async Task CalculateTaxes_JsonConverter_DeserializationError_ThrowsServiceInternalException()
        {
            string fromCountry = "US";
            string fromCity = "La Jolla";
            string fromState = "CA";
            string fromZip = "92093";
            string fromStreet = "9500 Gilman Drive";
            string toCountry = "US";
            string toCity = "Los Angeles";
            string toState = "CA";
            string toCounty = "LOS ANGELES";
            string toZip = "90002";
            string toStreet = "1335 E 103rd St";
            decimal orderTotal = 16.5m;
            decimal shipping = 1.5m;
            decimal taxAmount = 1.35m;

            IAddress fromAddressStub = Mock.Of<IAddress>(
                address => address.Country == fromCountry && address.City == fromCity && address.State == fromState && address.Zip == fromZip && address.StreetAddress == fromStreet
            );
            IAddress toAddressStub = Mock.Of<IAddress>(
                address => address.Country == toCountry && address.City == toCity && address.State == toState && address.Zip == toZip && address.StreetAddress == toStreet
            );

            string requestBody = TaxesRequestJson(fromCountry, fromZip, fromState, fromCity, fromStreet, toZip, toState, toCity, toStreet, orderTotal, shipping);
            string responseBody = TaxesResponseJson(orderTotal, shipping, taxAmount, true, false, "destination", toCountry, state: toState, county: toCounty, city: toCity.ToUpper());

            Mock<IJsonConverter> jsonConverterMock = new();
            jsonConverterMock.Setup(json => json.SerializeObject(It.IsAny<object>())).Returns(requestBody);
            jsonConverterMock.Setup(json => json.DeserializeObject<TaxesResponseWrapper>(It.IsAny<string>())).Throws<DeserializationException>();

            IHttpRestResponse response = Mock.Of<IHttpRestResponse>(
                response => response.IsSuccess == true && response.StatusCode == 200 && response.CodeReason == "OK"
                && response.GetBodyAsync().Result == responseBody);

            Mock<IHttpRestClient> restClientMock = new();
            restClientMock.Setup(client => client.JsonPostRequest(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<KeyValuePair<string, string>>?>()
            )).ReturnsAsync(response);

            TaxJarCalculator calculator = new(restClientMock.Object, jsonConverterMock.Object, "");

            await Assert.ThrowsExceptionAsync<ServiceInternalException>(() => calculator.CalculateTaxes(fromAddressStub, toAddressStub, orderTotal, shipping));
        }

        [TestMethod()]
        public async Task CalculateTaxes_RestHttpClient_RequestException_ThrowsServiceInternalException()
        {
            string fromCountry = "US";
            string fromCity = "La Jolla";
            string fromState = "CA";
            string fromZip = "92093";
            string fromStreet = "9500 Gilman Drive";
            string toCountry = "US";
            string toCity = "Los Angeles";
            string toState = "CA";
            string toZip = "90002";
            string toStreet = "1335 E 103rd St";
            decimal orderTotal = 16.5m;
            decimal shipping = 1.5m;

            IAddress fromAddressStub = Mock.Of<IAddress>(
                address => address.Country == fromCountry && address.City == fromCity && address.State == fromState && address.Zip == fromZip && address.StreetAddress == fromStreet
            );
            IAddress toAddressStub = Mock.Of<IAddress>(
                address => address.Country == toCountry && address.City == toCity && address.State == toState && address.Zip == toZip && address.StreetAddress == toStreet
            );

            Mock<IHttpRestClient> restClientMock = new();
            restClientMock.Setup(client => client.JsonPostRequest(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<KeyValuePair<string, string>>?>()
            )).ThrowsAsync(new RequestException());

            IJsonConverter converterStub = Mock.Of<IJsonConverter>();
            TaxJarCalculator calculator = new(restClientMock.Object, converterStub, "");

            await Assert.ThrowsExceptionAsync<ServiceInternalException>(() => calculator.CalculateTaxes(fromAddressStub, toAddressStub, orderTotal, shipping));
        }

        [TestMethod()]
        [DynamicData(nameof(AuthErrorResults), DynamicDataSourceType.Method)]
        public async Task CalculateTaxes_AuthUnsuccessfulResponse_ThrowsServiceConfigurationException(int errorCode, string errorMessage, string errorDetail)
        {
            string fromCountry = "US";
            string fromCity = "La Jolla";
            string fromState = "CA";
            string fromZip = "92093";
            string fromStreet = "9500 Gilman Drive";
            string toCountry = "US";
            string toCity = "Los Angeles";
            string toState = "CA";
            string toZip = "90002";
            string toStreet = "1335 E 103rd St";
            decimal orderTotal = 16.5m;
            decimal shipping = 1.5m;

            IAddress fromAddressStub = Mock.Of<IAddress>(
                address => address.Country == fromCountry && address.City == fromCity && address.State == fromState && address.Zip == fromZip && address.StreetAddress == fromStreet
            );
            IAddress toAddressStub = Mock.Of<IAddress>(
                address => address.Country == toCountry && address.City == toCity && address.State == toState && address.Zip == toZip && address.StreetAddress == toStreet
            );

            string responseJson = ErrorResponse(errorCode, errorMessage, errorDetail);
            IHttpRestResponse response = Mock.Of<IHttpRestResponse>(
                response => response.IsSuccess == false && response.StatusCode == errorCode && response.CodeReason == errorDetail &&
                response.GetBodyAsync().Result == responseJson
            );

            Mock<IHttpRestClient> restClientMock = new();
            restClientMock.Setup(client => client.JsonPostRequest(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<KeyValuePair<string, string>>?>()
            )).ReturnsAsync(response);

            ErrorResponse decodedResponse = new() { Detail = errorDetail, ErrorName = errorMessage, Status = errorCode.ToString() };

            IJsonConverter converterStub = Mock.Of<IJsonConverter>(converter => converter.DeserializeObject<ErrorResponse>(responseJson) == decodedResponse);

            TaxJarCalculator calculator = new(restClientMock.Object, converterStub, "");

            await Assert.ThrowsExceptionAsync<ServiceConfigurationException>(() => calculator.CalculateTaxes(fromAddressStub, toAddressStub, orderTotal, shipping));

        }


        [TestMethod()]
        [DynamicData(nameof(DefinedErrorResults), DynamicDataSourceType.Method)]
        public async Task CalculateTaxes_ErrorResponse_ThrowsServiceInternalException(int errorCode, string errorMessage, string errorDetail)
        {
            string fromCountry = "US";
            string fromCity = "La Jolla";
            string fromState = "CA";
            string fromZip = "92093";
            string fromStreet = "9500 Gilman Drive";
            string toCountry = "US";
            string toCity = "Los Angeles";
            string toState = "CA";
            string toZip = "90002";
            string toStreet = "1335 E 103rd St";
            decimal orderTotal = 16.5m;
            decimal shipping = 1.5m;

            IAddress fromAddressStub = Mock.Of<IAddress>(
                address => address.Country == fromCountry && address.City == fromCity && address.State == fromState && address.Zip == fromZip && address.StreetAddress == fromStreet
            );
            IAddress toAddressStub = Mock.Of<IAddress>(
                address => address.Country == toCountry && address.City == toCity && address.State == toState && address.Zip == toZip && address.StreetAddress == toStreet
            );

            string responseJson = ErrorResponse(errorCode, errorMessage, errorDetail);
            IHttpRestResponse response = Mock.Of<IHttpRestResponse>(
                response => response.IsSuccess == false && response.StatusCode == errorCode && response.CodeReason == errorDetail &&
                response.GetBodyAsync().Result == responseJson
            );

            Mock<IHttpRestClient> restClientMock = new();
            restClientMock.Setup(client => client.JsonPostRequest(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<KeyValuePair<string, string>>?>()
            )).ReturnsAsync(response);

            ErrorResponse decodedResponse = new() { Detail = errorDetail, ErrorName = errorMessage, Status = errorCode.ToString() };

            IJsonConverter converterStub = Mock.Of<IJsonConverter>(converter => converter.DeserializeObject<ErrorResponse>(responseJson) == decodedResponse);

            TaxJarCalculator calculator = new(restClientMock.Object, converterStub, "");

            await Assert.ThrowsExceptionAsync<ServiceInternalException>(() => calculator.CalculateTaxes(fromAddressStub, toAddressStub, orderTotal, shipping));
        }

        [TestMethod()]
        public async Task CalculateTaxes_OtherInvalidResponse_ThrowsServiceInternalException()
        {
            string fromCountry = "US";
            string fromCity = "La Jolla";
            string fromState = "CA";
            string fromZip = "92093";
            string fromStreet = "9500 Gilman Drive";
            string toCountry = "US";
            string toCity = "Los Angeles";
            string toState = "CA";
            string toZip = "90002";
            string toStreet = "1335 E 103rd St";
            decimal orderTotal = 16.5m;
            decimal shipping = 1.5m;

            IAddress fromAddressStub = Mock.Of<IAddress>(
                address => address.Country == fromCountry && address.City == fromCity && address.State == fromState && address.Zip == fromZip && address.StreetAddress == fromStreet
            );
            IAddress toAddressStub = Mock.Of<IAddress>(
                address => address.Country == toCountry && address.City == toCity && address.State == toState && address.Zip == toZip && address.StreetAddress == toStreet
            );

            IHttpRestResponse response = Mock.Of<IHttpRestResponse>(
                response => response.IsSuccess == false && response.StatusCode == 410 && response.CodeReason == "I'm a Teapot" &&
                response.GetBodyAsync().Result == ""
            );

            Mock<IHttpRestClient> restClientMock = new();
            restClientMock.Setup(client => client.JsonPostRequest(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<KeyValuePair<string, string>>?>()
            )).ReturnsAsync(response);

            Mock<IJsonConverter> converterMock = new();
            converterMock.Setup(converter => converter.DeserializeObject<ErrorResponse>("")).Throws<DeserializationException>();

            TaxJarCalculator calculator = new(restClientMock.Object, converterMock.Object, "");

            await Assert.ThrowsExceptionAsync<ServiceInternalException>(() => calculator.CalculateTaxes(fromAddressStub, toAddressStub, orderTotal, shipping));

        }

        [TestMethod()]
        public void CalculateTaxes_ToUsAddress_CorrectDecimal()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void CalculateTaxes_ToCaAddress_CorrectDecimal()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void CalculateTaxes_ToAuAddress_CorrectDecimal()
        {
            Assert.Fail();
        }


        [TestMethod()]
        public void CalculateTaxes_ToEuAddress_CorrectDecimal()
        {
            Assert.Fail();
        }
        #endregion

        #region IsCountryEu
        [TestMethod()]
        [DynamicData(nameof(ExpectedEuCountriesData), DynamicDataSourceType.Method)]
        public void IsCountryEu_EuCountry_ReturnsTrue(string country)
        {
            TaxJarCalculator calculator = CalculatorWithStubs();
            Assert.IsTrue(calculator.IsCountryEu(country));
        }

        [TestMethod()]
        [DynamicData(nameof(NonEuCountriesData), DynamicDataSourceType.Method)]
        public void IsCountryEu_NonEuSupportedCountry_ReturnsFalse(string country)
        {
            TaxJarCalculator calculator = CalculatorWithStubs();
            Assert.IsFalse(calculator.IsCountryEu(country));
        }

        [TestMethod()]
        [DynamicData(nameof(InvalidCountryCodesDataIncludeEmpty), DynamicDataSourceType.Method)]
        public void IsCountryEu_InvalidCountry_ReturnsFalse(string country)
        {
            TaxJarCalculator calculator = CalculatorWithStubs();
            Assert.IsFalse(calculator.IsCountryEu(country));
        }
        #endregion

        #region IsValidCountry
        [TestMethod()]
        [DynamicData(nameof(AllExpectedCountriesData), DynamicDataSourceType.Method)]
        public void IsValidCountry_ValidCountry_ReturnsTrue(string country)
        {
            TaxJarCalculator calculator = CalculatorWithStubs();
            Assert.IsTrue(calculator.IsValidCountry(country));
        }

        [TestMethod()]
        [DynamicData(nameof(InvalidCountryCodesDataIncludeEmpty), DynamicDataSourceType.Method)]
        public void IsValidCountry_InvalidCountry_ReturnsFalse(string country)
        {
            TaxJarCalculator calculator = CalculatorWithStubs();
            Assert.IsFalse(calculator.IsValidCountry(country));
        }

        [TestMethod()]
        [DynamicData(nameof(UnsupportedCountryCodesData), DynamicDataSourceType.Method)]
        public void IsValidCountry_UnsupportedCountry_ReturnsFalse(string country)
        {
            TaxJarCalculator calculator = CalculatorWithStubs();
            Assert.IsFalse(calculator.IsValidCountry(country));
        }
        #endregion

        #region SupportedCountries
        [TestMethod()]
        public void SupportedCountries_ReturnsExpectedCountries()
        {
            TaxJarCalculator calculator = CalculatorWithStubs();
            IEnumerable<string> countries = calculator.SupportedCountries();
            Assert.IsFalse(countries.Except(GetAllExpectedCountries()).Any());
        }
        #endregion
    }
}