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
using TaxCalculator.Services.TaxCalculators.TaxJar.Requests;

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

        private const string ratesUri = "https://api.taxjar.com/v2/rates/{0}";
        private const string taxesUri = "https://api.taxjar.com/v2/taxes";
        private const string apiKey = "9e0cd62a22f451701f29c3bde214";
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

        private static string TaxesRequestJson(string? fromCountry, string? fromZip, string? fromState, string? fromCity, string? fromStreet, 
            string toCountry, string? toZip, string? toState, string? toCity, string? toStreet, decimal amount, decimal shipping)
        {
            StringBuilder optionals = new();
            string optionTemplate = ",\n\"{0}\": \"{1}\"";
            if (fromCountry != null)
            {
                optionals.Append(string.Format(optionTemplate, "from_country", fromCountry));
            }
            if (fromState != null)
            {
                optionals.Append(string.Format(optionTemplate, "from_state", fromState));
            }
            if (toState != null)
            {
                optionals.Append(string.Format(optionTemplate, "to_state", toState));
            }
            if (fromCity != null)
            {
                optionals.Append(string.Format(optionTemplate, "from_city", fromCity));
            }
            if (toCity != null)
            {
                optionals.Append(string.Format(optionTemplate, "to_city", toCity));
            }
            if (fromStreet != null)
            {
                optionals.Append(string.Format(optionTemplate, "from_street", fromStreet));
            }
            if (toStreet != null)
            {
                optionals.Append(string.Format(optionTemplate, "to_street", toStreet));
            }
            if (fromZip != null)
            {
                optionals.Append(string.Format(optionTemplate, "from_zip", fromZip));
            }
            if (toZip != null)
            {
                optionals.Append(string.Format(optionTemplate, "to_zip", toZip));
            }


            return $@"{{
""to_country"": ""{toCountry}"",
""amount"": {amount},
""shipping"": {shipping}{optionals}
}}";
        }

        private static string TaxesResponseJson(decimal orderTotal, decimal shipping, decimal taxableAmount, decimal amountToCollect, decimal rate, bool hasNexus, 
            bool freightTaxable, string? taxSource, 
            string country, string? state = null, string? county = null, string? city = null)
        {
            string taxSourceJson = taxSource == null ? "null" : '"' + taxSource + '"';

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
            jurisdications.Append('}');

            return @$"{{""tax"": {{
""order_total_amount"": {orderTotal},
""shipping"": {shipping},
""taxable_amount"": {taxableAmount},
""amount_to_collect"": {amountToCollect},
""rate"": {rate},
""hax_nexus"": {(hasNexus ? "true" : "false" )},
""freight_taxable"": {(freightTaxable ? "true" : "false")},
""tax_source"": {taxSourceJson},
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

        private static bool TaxRequestMatches(TaxesRequest value, string? fromCity, string? fromStreet, string? fromState, string? fromCountry, string? fromZip,
            string? toCity, string? toStreet, string? toState, string toCountry, string? toZip, decimal orderAmount, decimal shipping)
        {
            return value.FromCity == fromCity && value.FromStreet == fromStreet && value.FromState == fromState && value.FromCountry == fromCountry && value.FromZip == fromZip &&
                value.ToCity == toCity && value.ToStreet == toStreet && value.ToState == toState && value.ToCountry == toCountry && value.ToZip == toZip &&
                value.Amount == orderAmount && value.Shipping == shipping;
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
            string country = "US";
            string city = "Santa Monica";
            string state = "CA";
            string zip = "90404";
           
            Mock<IJsonConverter> jsonConverterMock = new();
            jsonConverterMock.Setup(json => json.DeserializeObject<object>(It.IsAny<string>())).Throws<DeserializationException>();

            IHttpRestResponse response = Mock.Of<IHttpRestResponse>(
                response => response.IsSuccess == true && response.StatusCode == 200 && response.CodeReason == "OK"
                && response.GetBodyAsync().Result == "");

            Mock<IHttpRestClient> restClientMock = new();
            restClientMock.Setup(client => client.GetRequest(
                It.IsAny<string>(), It.IsAny<IEnumerable<KeyValuePair<string, string>>?>(), It.IsAny<IEnumerable<KeyValuePair<string, string>>?>()
            )).ReturnsAsync(response);

            IAddress addressStub = Mock.Of<IAddress>(
                address => address.Country == country && address.City == city && address.State == state && address.Zip == zip && address.StreetAddress == null
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
            converterMock.Setup(converter => converter.DeserializeObject<ErrorResponse>(It.IsAny<string>())).Throws<DeserializationException>();

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
            string state = "CA";
            decimal stateRate = 0.0625m;
            string county = "LOS ANGELES";
            decimal countyRate = 0.01m;
            string city = "SANTA MONICA";
            decimal cityRate = 0;
            decimal combinedDistrictRate = 0.025m;
            decimal combinedRate = 0.0975m;
            bool freightTaxable = false;
            string responseBody = UsRateResponseJson(zip, state, stateRate, county, countyRate, city, cityRate, combinedDistrictRate, combinedRate, freightTaxable);
            RatesResponseWrapper ratesResponse = new()
            {
                Rate = new()
                {
                    Zip = zip,
                    State = state,
                    StateRate = stateRate,
                    County = county,
                    CountyRate = countyRate,
                    City = city,
                    CityRate = cityRate,
                    TotalDistrictRate = combinedDistrictRate,
                    TotalTaxRate = combinedRate,
                    FreightTaxable = freightTaxable
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


            Assert.AreEqual(combinedRate, await calculator.GetTaxRate(addressStub));
        }

        [TestMethod()]
        [DataRow(null)]
        [DataRow("2022-01-24")]
        public async Task GetTaxRate_UsAddress_CorrectDecimal(string? version)
        {
            string zip = "05495-2086";
            string country = "US";
            decimal countryRate = 0m;
            string state = "VT";
            decimal stateRate = 0.06m;
            string county = "CHITTENDEN";
            decimal countyRate = 0;
            string city = "Williston";
            decimal cityRate = 0;
            string street = "312 Hurricane Lane";
            decimal combinedDistrictRate = 0.01m;
            decimal combinedRate = 0.07m;
            bool freightTaxable = true;
            string responseBody = UsRateResponseWithCountry(zip, country, countryRate, state, stateRate, county, countyRate, city.ToUpper(), cityRate, combinedDistrictRate, 
                combinedRate, freightTaxable);
            RatesResponseWrapper ratesResponse = new()
            {
                Rate = new()
                {
                    Zip = zip,
                    Country = country,
                    CountryRate = countryRate,
                    State = state,
                    StateRate = stateRate,
                    County = county,
                    CountyRate = countyRate,
                    City = city.ToUpper(),
                    CityRate = cityRate,
                    TotalDistrictRate = combinedDistrictRate,
                    TotalTaxRate = combinedRate,
                    FreightTaxable = freightTaxable
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

            Assert.AreEqual(combinedRate, await calculator.GetTaxRate(addressStub));
        }

        [TestMethod()]
        public async Task GetTaxRate_CaAddress_CorrectDecimal()
        {
            string zip = "M5V 1J1";
            string country = "CA";
            string state = "ON";
            string city = "Toronto";
            string street = "1 Blue Jays Way";
            decimal combinedTaxRate = 0.13m;
            bool freightTaxable = true;
            string responseBody = CaRateResponseJson(zip, city, state, country, combinedTaxRate, freightTaxable);
            RatesResponseWrapper ratesResponse = new()
            {
                Rate = new()
                {
                    Zip = zip,
                    Country = country,
                    State = state,
                    City = city,
                    TotalTaxRate = combinedTaxRate,
                    FreightTaxable = freightTaxable
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

            Assert.AreEqual(combinedTaxRate, await calculator.GetTaxRate(addressStub));
        }

        [TestMethod()]
        public async Task GetTaxRate_AuAddress_CorrectDecimal()
        {
            string zip = "2000";
            string country = "AU";
            decimal countryRate = 0.1m;
            string state = "NSW";
            string city = "Sydney";
            string street = "2 Macquarie Street";
            decimal combinedTaxRate = 0.1m;
            bool freightTaxable = true;
            string responseBody = AuRateResponseJson(zip, country, 0.1m, 0.1m, true);
            RatesResponseWrapper ratesResponse = new()
            {
                Rate = new()
                {
                    Zip = zip,
                    Country = country,
                    CountryRate = countryRate,
                    TotalTaxRate = combinedTaxRate,
                    FreightTaxable = freightTaxable
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

            Assert.AreEqual(combinedTaxRate, await calculator.GetTaxRate(addressStub));
        }

        [TestMethod()]
        public async Task GetTaxRate_EuAddress_CorrectDecimal()
        {
            string zip = "75015";
            string country = "FR";
            string countryName = "France";
            string city = "Paris";
            string street = "156 Boulevard de Grenelle";
            decimal standardRate = 0.2m;
            decimal reducedRate = 0.1m;
            decimal superReducedRate = 0.055m;
            decimal parkingRate = 0;
            decimal distanceSaleThreshold = 0;
            bool freightTaxable = true;
            string responseBody = EuRateResponseJson(country, countryName, standardRate, reducedRate, superReducedRate, parkingRate, distanceSaleThreshold, freightTaxable);
            RatesResponseWrapper ratesResponse = new()
            {
                Rate = new()
                {
                    Country = country,
                    CountryName = countryName,
                    StandardRate = standardRate,
                    ReducedRate = reducedRate,
                    SuperReducedRate = superReducedRate,
                    DistanceSaleThreshold = distanceSaleThreshold,
                    FreightTaxable = freightTaxable
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

            Assert.AreEqual(standardRate, await calculator.GetTaxRate(addressStub));
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
            decimal orderAmount = 15m;
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

            await Assert.ThrowsExceptionAsync<ServiceInternalException>(() => calculator.CalculateTaxes(fromAddressStub, toAddressStub, orderAmount, shipping));
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
            string toZip = "90002";
            string toStreet = "1335 E 103rd St";
            decimal orderAmount = 15m;
            decimal shipping = 1.5m;

            IAddress fromAddressStub = Mock.Of<IAddress>(
                address => address.Country == fromCountry && address.City == fromCity && address.State == fromState && address.Zip == fromZip && address.StreetAddress == fromStreet
            );
            IAddress toAddressStub = Mock.Of<IAddress>(
                address => address.Country == toCountry && address.City == toCity && address.State == toState && address.Zip == toZip && address.StreetAddress == toStreet
            );

            Mock<IJsonConverter> jsonConverterMock = new();
            jsonConverterMock.Setup(json => json.DeserializeObject<TaxesResponseWrapper>(It.IsAny<string>())).Throws<DeserializationException>();

            IHttpRestResponse response = Mock.Of<IHttpRestResponse>(
                response => response.IsSuccess == true && response.StatusCode == 200 && response.CodeReason == "OK"
                && response.GetBodyAsync().Result == "");

            Mock<IHttpRestClient> restClientMock = new();
            restClientMock.Setup(client => client.JsonPostRequest(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<KeyValuePair<string, string>>?>()
            )).ReturnsAsync(response);

            TaxJarCalculator calculator = new(restClientMock.Object, jsonConverterMock.Object, "");

            await Assert.ThrowsExceptionAsync<ServiceInternalException>(() => calculator.CalculateTaxes(fromAddressStub, toAddressStub, orderAmount, shipping));
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
            decimal orderAmount = 15m;
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

            await Assert.ThrowsExceptionAsync<ServiceInternalException>(() => calculator.CalculateTaxes(fromAddressStub, toAddressStub, orderAmount, shipping));
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
            decimal orderAmount = 15m;
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

            await Assert.ThrowsExceptionAsync<ServiceConfigurationException>(() => calculator.CalculateTaxes(fromAddressStub, toAddressStub, orderAmount, shipping));

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
            decimal orderAmount = 15m;
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

            await Assert.ThrowsExceptionAsync<ServiceInternalException>(() => calculator.CalculateTaxes(fromAddressStub, toAddressStub, orderAmount, shipping));
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
            decimal orderAmount = 15m;
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

            await Assert.ThrowsExceptionAsync<ServiceInternalException>(() => calculator.CalculateTaxes(fromAddressStub, toAddressStub, orderAmount, shipping));

        }

        [TestMethod()]
        [DataRow(null)]
        [DataRow("")]
        public async Task CalculateTaxes_AuRequiredAddressOnly_CorrectDecimal(string? otherElement)
        {
            string fromCountry = "AU";
            string toCountry = "AU";
            decimal orderAmount = 15m;
            decimal shipping = 1.5m;
            decimal orderTotal = orderAmount + shipping;
            decimal taxAmount = 1.65m;
            bool hasNexus = true;
            bool freightTaxable = true;
            decimal taxRate = 0.1m;
            string taxSource = "destination";

            IAddress? fromAddressStub = null;
            IAddress toAddressStub = Mock.Of<IAddress>(
                address => address.Country == toCountry && address.City == otherElement && address.State == otherElement && address.Zip == otherElement && address.StreetAddress == otherElement
            );
 
            string requestBody = TaxesRequestJson(fromCountry, null, null, null, null, toCountry, null, null, null, null, orderAmount, shipping);
            string responseBody = TaxesResponseJson(orderTotal, shipping, orderTotal, taxAmount, taxRate, hasNexus, freightTaxable, taxSource, toCountry);
            TaxesResponseWrapper deserializedResponse = new()
            {
                Tax = new TaxesResponse()
                {
                    OrderTotal = orderTotal,
                    Shipping = shipping,
                    TaxableAmount = orderTotal,
                    TaxToCollect = taxAmount,
                    TaxRate = taxRate,
                    HasNexus = hasNexus,
                    FreightTaxable = freightTaxable,
                    TaxSource = taxSource,
                    Jurisdictions = new TaxJurisdiction()
                    {
                        Country = toCountry
                    }
                }
            };

            Mock<IJsonConverter> jsonConverterMock = new();
            jsonConverterMock.Setup(json => json.SerializeObject(It.Is<TaxesRequest>(
                value => TaxRequestMatches(value, null, null, null, null, null, null, null, null, toCountry, null, orderAmount, shipping)
            ))).Returns(requestBody);
            jsonConverterMock.Setup(json => json.DeserializeObject<TaxesResponseWrapper>(It.Is<string>(val => val == responseBody))).Returns(deserializedResponse);

            IHttpRestResponse response = Mock.Of<IHttpRestResponse>(
                response => response.IsSuccess == true && response.StatusCode == 200 && response.CodeReason == "OK"
                && response.GetBodyAsync().Result == responseBody);

            Mock<IHttpRestClient> restClientMock = new();
            restClientMock.Setup(client => client.JsonPostRequest(
                It.Is<string>(val => val == taxesUri), It.Is<string>(val => val == requestBody),
                It.Is<IEnumerable<KeyValuePair<string, string>>>(val => CheckHeadersExpected(val, apiKey, null))
            )).ReturnsAsync(response);

            TaxJarCalculator calculator = new(restClientMock.Object, jsonConverterMock.Object, apiKey);

            Assert.AreEqual(taxAmount, await calculator.CalculateTaxes(fromAddressStub, toAddressStub, orderAmount, shipping));
        }

        [TestMethod()]
        [DataRow(null)]
        [DataRow("2022-01-24")]
        public async Task CalculateTaxes_ToUsAddress_CorrectDecimal(string? version)
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
            decimal orderAmount = 15m;
            decimal shipping = 1.5m;
            decimal orderTotal = orderAmount + shipping;
            decimal taxAmount = 1.35m;
            decimal taxRate = 0.09m;
            bool hasNexus = true;
            bool freightTaxable = true;
            string taxSource = "destination";

            IAddress fromAddressStub = Mock.Of<IAddress>(
                address => address.Country == fromCountry && address.City == fromCity && address.State == fromState && address.Zip == fromZip && address.StreetAddress == fromStreet
            );
            IAddress toAddressStub = Mock.Of<IAddress>(
                address => address.Country == toCountry && address.City == toCity && address.State == toState && address.Zip == toZip && address.StreetAddress == toStreet
            );

            string requestBody = TaxesRequestJson(fromCountry, fromZip, fromState, fromCity, fromStreet, toCountry, toZip, toState, toCity, toStreet, orderAmount, shipping);
            string responseBody = TaxesResponseJson(orderTotal, shipping, orderAmount, taxAmount, taxRate, hasNexus, freightTaxable, taxSource, toCountry, state: toState, county: toCounty, city: toCity.ToUpper());
            TaxesResponseWrapper deserializedResponse = new()
            {
                Tax = new TaxesResponse()
                {
                    OrderTotal = orderTotal,
                    Shipping = shipping,
                    TaxableAmount = orderAmount,
                    TaxToCollect = taxAmount,
                    TaxRate = taxRate,
                    HasNexus = hasNexus,
                    FreightTaxable = freightTaxable,
                    TaxSource = taxSource,
                    Jurisdictions = new TaxJurisdiction()
                    {
                        Country = toCountry,
                        State = toState,
                        County = toCounty,
                        City = toCity.ToUpper()
                    }
                }
            };

            Mock<IJsonConverter> jsonConverterMock = new();
            jsonConverterMock.Setup(json => json.SerializeObject(It.Is<TaxesRequest>(
                value => TaxRequestMatches(value, fromCity, fromStreet, fromState, fromCountry, fromZip, toCity, toStreet, toState, toCountry, toZip, orderAmount, shipping)
            ))).Returns(requestBody);
            jsonConverterMock.Setup(json => json.DeserializeObject<TaxesResponseWrapper>(It.Is<string>(val => val == responseBody))).Returns(deserializedResponse);

            IHttpRestResponse response = Mock.Of<IHttpRestResponse>(
                response => response.IsSuccess == true && response.StatusCode == 200 && response.CodeReason == "OK"
                && response.GetBodyAsync().Result == responseBody);

            Mock<IHttpRestClient> restClientMock = new();
            restClientMock.Setup(client => client.JsonPostRequest(
                It.Is<string>(val => val == taxesUri), It.Is<string>(val => val == requestBody), 
                It.Is<IEnumerable<KeyValuePair<string, string>>>(val => CheckHeadersExpected(val, apiKey, version))
            )).ReturnsAsync(response);

            TaxJarCalculator calculator = new(restClientMock.Object, jsonConverterMock.Object, apiKey, version);

            Assert.AreEqual(taxAmount, await calculator.CalculateTaxes(fromAddressStub, toAddressStub, orderAmount, shipping));
        }

        [TestMethod()]
        public async Task CalculateTaxes_ToCaAddress_CorrectDecimal()
        {
            string fromCountry = "CA";
            string fromCity = "Toronto";
            string fromState = "ON";
            string fromZip = "M5V 1J1";
            string fromStreet = "1 Blue Jays Way";
            string toCountry = "CA";
            string toCity = "Montreal";
            string toState = "QC";
            string toZip = "H3C 5L2";
            string toStreet = "1909 Av des Canadiens-de-Montreal";
            decimal orderAmount = 15m;
            decimal shipping = 1.5m;
            decimal orderTotal = orderAmount + shipping;
            decimal taxAmount = 0.83m;
            decimal taxRate = 0.05m;
            bool hasNexus = true;
            bool freightTaxable = true;
            string taxSource = "destination";

            IAddress fromAddressStub = Mock.Of<IAddress>(
                address => address.Country == fromCountry && address.City == fromCity && address.State == fromState && address.Zip == fromZip && address.StreetAddress == fromStreet
            );
            IAddress toAddressStub = Mock.Of<IAddress>(
                address => address.Country == toCountry && address.City == toCity && address.State == toState && address.Zip == toZip && address.StreetAddress == toStreet
            );

            string requestBody = TaxesRequestJson(fromCountry, fromZip, fromState, fromCity, fromStreet, toCountry, toZip, toState, toCity, toStreet, orderAmount, shipping);
            string responseBody = TaxesResponseJson(orderTotal, shipping, orderTotal, taxAmount, taxRate, hasNexus, freightTaxable, taxSource, toCountry, state: toState, city: toCity.ToUpper());
            TaxesResponseWrapper deserializedResponse = new()
            {
                Tax = new TaxesResponse()
                {
                    OrderTotal = orderTotal,
                    Shipping = shipping,
                    TaxableAmount = orderTotal,
                    TaxToCollect = taxAmount,
                    TaxRate = taxRate,
                    HasNexus = hasNexus,
                    FreightTaxable = freightTaxable,
                    TaxSource = taxSource,
                    Jurisdictions = new TaxJurisdiction()
                    {
                        Country = toCountry,
                        State = toState,
                        City = toCity.ToUpper()
                    }
                }
            };

            Mock<IJsonConverter> jsonConverterMock = new();
            jsonConverterMock.Setup(json => json.SerializeObject(It.Is<TaxesRequest>(
                value => TaxRequestMatches(value, fromCity, fromStreet, fromState, fromCountry, fromZip, toCity, toStreet, toState, toCountry, toZip, orderAmount, shipping)
            ))).Returns(requestBody);
            jsonConverterMock.Setup(json => json.DeserializeObject<TaxesResponseWrapper>(It.Is<string>(val => val == responseBody))).Returns(deserializedResponse);

            IHttpRestResponse response = Mock.Of<IHttpRestResponse>(
                response => response.IsSuccess == true && response.StatusCode == 200 && response.CodeReason == "OK"
                && response.GetBodyAsync().Result == responseBody);

            Mock<IHttpRestClient> restClientMock = new();
            restClientMock.Setup(client => client.JsonPostRequest(
                It.Is<string>(val => val == taxesUri), It.Is<string>(val => val == requestBody),
                It.Is<IEnumerable<KeyValuePair<string, string>>>(val => CheckHeadersExpected(val, apiKey, null))
            )).ReturnsAsync(response);

            TaxJarCalculator calculator = new(restClientMock.Object, jsonConverterMock.Object, apiKey);

            Assert.AreEqual(taxAmount, await calculator.CalculateTaxes(fromAddressStub, toAddressStub, orderAmount, shipping));
        }

        [TestMethod()]
        public async Task CalculateTaxes_ToAuAddress_CorrectDecimal()
        {
            string fromCountry = "AU";
            string fromCity = "Sydney";
            string fromState = "NSW";
            string fromZip = "2000";
            string fromStreet = "2 Macquarie Street";
            string toCountry = "AU";
            string toCity = "Canberra";
            string toState = "ACT";
            string toZip = "2600";
            string toStreet = "Parliament Drive";
            decimal orderAmount = 15m;
            decimal shipping = 1.5m;
            decimal orderTotal = orderAmount + shipping;
            decimal taxAmount = 1.65m;
            decimal taxRate = 0.1m;
            bool hasNexus = true;
            bool freightTaxable = true;
            string taxSource = "destination";

            IAddress fromAddressStub = Mock.Of<IAddress>(
                address => address.Country == fromCountry && address.City == fromCity && address.State == fromState && address.Zip == fromZip && address.StreetAddress == fromStreet
            );
            IAddress toAddressStub = Mock.Of<IAddress>(
                address => address.Country == toCountry && address.City == toCity && address.State == toState && address.Zip == toZip && address.StreetAddress == toStreet
            );

            string requestBody = TaxesRequestJson(fromCountry, fromZip, fromState, fromCity, fromStreet, toCountry, toZip, toState, toCity, toStreet, orderAmount, shipping);
            string responseBody = TaxesResponseJson(orderTotal, shipping, orderTotal, taxAmount, taxRate, hasNexus, freightTaxable, taxSource, toCountry);
            TaxesResponseWrapper deserializedResponse = new()
            {
                Tax = new TaxesResponse()
                {
                    OrderTotal = orderTotal,
                    Shipping = shipping,
                    TaxableAmount = orderTotal,
                    TaxToCollect = taxAmount,
                    TaxRate = taxRate,
                    HasNexus = hasNexus,
                    FreightTaxable = freightTaxable,
                    TaxSource = taxSource,
                    Jurisdictions = new TaxJurisdiction()
                    {
                        Country = toCountry
                    }
                }
            };

            Mock<IJsonConverter> jsonConverterMock = new();
            jsonConverterMock.Setup(json => json.SerializeObject(It.Is<TaxesRequest>(
                value => TaxRequestMatches(value, fromCity, fromStreet, fromState, fromCountry, fromZip, toCity, toStreet, toState, toCountry, toZip, orderAmount, shipping)
            ))).Returns(requestBody);
            jsonConverterMock.Setup(json => json.DeserializeObject<TaxesResponseWrapper>(It.Is<string>(val => val == responseBody))).Returns(deserializedResponse);

            IHttpRestResponse response = Mock.Of<IHttpRestResponse>(
                response => response.IsSuccess == true && response.StatusCode == 200 && response.CodeReason == "OK"
                && response.GetBodyAsync().Result == responseBody);

            Mock<IHttpRestClient> restClientMock = new();
            restClientMock.Setup(client => client.JsonPostRequest(
                It.Is<string>(val => val == taxesUri), It.Is<string>(val => val == requestBody),
                It.Is<IEnumerable<KeyValuePair<string, string>>>(val => CheckHeadersExpected(val, apiKey, null))
            )).ReturnsAsync(response);

            TaxJarCalculator calculator = new(restClientMock.Object, jsonConverterMock.Object, apiKey);

            Assert.AreEqual(taxAmount, await calculator.CalculateTaxes(fromAddressStub, toAddressStub, orderAmount, shipping));
        }


        [TestMethod()]
        public async Task CalculateTaxes_ToEuAddress_CorrectDecimal()
        {
            string fromCountry = "FR";
            string fromCity = "Paris";
            string fromZip = "75007";
            string fromStreet = "5 Avenue Anatole";
            string toCountry = "FR";
            string toCity = "Paris";
            string toZip = "75001";
            string toStreet = "8P Place du Carrousel";
            decimal orderAmount = 15m;
            decimal shipping = 1.5m;
            decimal orderTotal = orderAmount + shipping;
            decimal taxAmount = 3.3m;
            decimal taxRate = 0.2m;
            bool hasNexus = true;
            bool freightTaxable = true;
            string taxSource = "destination";

            IAddress fromAddressStub = Mock.Of<IAddress>(
                address => address.Country == fromCountry && address.City == fromCity && address.State == "" && address.Zip == fromZip && address.StreetAddress == fromStreet
            );
            IAddress toAddressStub = Mock.Of<IAddress>(
                address => address.Country == toCountry && address.City == toCity && address.State == "" && address.Zip == toZip && address.StreetAddress == toStreet
            );

            string requestBody = TaxesRequestJson(fromCountry, fromZip, null, fromCity, fromStreet, toCountry, toZip, null, toCity, toStreet, orderAmount, shipping);
            string responseBody = TaxesResponseJson(orderTotal, shipping, orderTotal, taxAmount, taxRate, hasNexus, freightTaxable, taxSource, toCountry, city: toCity);
            TaxesResponseWrapper deserializedResponse = new()
            {
                Tax = new TaxesResponse()
                {
                    OrderTotal = orderTotal,
                    Shipping = shipping,
                    TaxableAmount = orderTotal,
                    TaxToCollect = taxAmount,
                    TaxRate = taxRate,
                    HasNexus = hasNexus,
                    FreightTaxable = freightTaxable,
                    TaxSource = taxSource,
                    Jurisdictions = new TaxJurisdiction()
                    {
                        Country = toCountry,
                        City = toCity.ToUpper()
                    }
                }
            };

            Mock<IJsonConverter> jsonConverterMock = new();
            jsonConverterMock.Setup(json => json.SerializeObject(It.Is<TaxesRequest>(
                value => TaxRequestMatches(value, fromCity, fromStreet, null, fromCountry, fromZip, toCity, toStreet, null, toCountry, toZip, orderAmount, shipping)
            ))).Returns(requestBody);
            jsonConverterMock.Setup(json => json.DeserializeObject<TaxesResponseWrapper>(It.Is<string>(val => val == responseBody))).Returns(deserializedResponse);

            IHttpRestResponse response = Mock.Of<IHttpRestResponse>(
                response => response.IsSuccess == true && response.StatusCode == 200 && response.CodeReason == "OK"
                && response.GetBodyAsync().Result == responseBody);

            Mock<IHttpRestClient> restClientMock = new();
            restClientMock.Setup(client => client.JsonPostRequest(
                It.Is<string>(val => val == taxesUri), It.Is<string>(val => val == requestBody),
                It.Is<IEnumerable<KeyValuePair<string, string>>>(val => CheckHeadersExpected(val, apiKey, null))
            )).ReturnsAsync(response);

            TaxJarCalculator calculator = new(restClientMock.Object, jsonConverterMock.Object, apiKey);

            Assert.AreEqual(taxAmount, await calculator.CalculateTaxes(fromAddressStub, toAddressStub, orderAmount, shipping));
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