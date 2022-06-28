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

namespace TaxCalculator.Services.TaxCalculators.TaxJar.Tests
{
    [TestClass()]
    public class TaxJarCalculatorTests
    {
        private static readonly string[] expectedEuCountries = new string[]
        {
            "AT", "BE", "BG", "HR", "CY", "CZ", "DK", "EE", "FI", "FR", "DE", "GR", "HU", "IE",
            "IT", "LV", "LT", "LU", "MT", "NL", "PL", "PT", "RO", "SK", "SI", "ES", "SE", "GB"
        };
        private static readonly string[] expectedNonEuCountries = new string[] { "US", "CA", "AU" };

        private static readonly string[] nonCountryCodes = new string[] { "A", "AB", "ABC" };
        private static readonly string[] unsupportedCountryCodes = new string[] { "BR", "EG", "CN" };

        private static object[] StringToObjectArray(string input) => new object[] { input };

        private static IEnumerable<string> GetAllExpectedCountries() => expectedNonEuCountries.Concat(expectedEuCountries);
        private static IEnumerable<object[]> AllExpectedCountriesData() => GetAllExpectedCountries().Select(StringToObjectArray);
        private static IEnumerable<object[]> ExpectedEuCountriesData() => expectedEuCountries.Select(StringToObjectArray);
        private static IEnumerable<object[]> NonEuCountriesData() => expectedNonEuCountries.Select(StringToObjectArray);
        private static IEnumerable<object[]> InvalidCountryCodesDataIncludeBlank() => nonCountryCodes.Append("").Select(StringToObjectArray);
        private static IEnumerable<object[]> InvalidCountryCodesData() => nonCountryCodes.Where(x => x != "").Select(StringToObjectArray);
        private static IEnumerable<object[]> UnsupportedCountryCodesData() => unsupportedCountryCodes.Select(StringToObjectArray);

        private static TaxJarCalculator CalculatorWithStubs() 
        {
            IHttpRestClient restClientStub = Mock.Of<IHttpRestClient>(MockBehavior.Strict);
            IJsonConverter jsonConverterStub = Mock.Of<IJsonConverter>(MockBehavior.Strict);
            return new(restClientStub, jsonConverterStub, "");
        }

        #region GetTaxRate
        [TestMethod()]
        public async Task GetTaxRate_NullToAddress_ThrowsServiceInputException()
        {
            TaxJarCalculator calculator = CalculatorWithStubs();
            await Assert.ThrowsExceptionAsync<ServiceInputException>(() => calculator.GetTaxRate(null));
        }

        [TestMethod()]
        public async Task GetTaxRate_NullToZip_ThrowsServiceInputException()
        {
           TaxJarCalculator calculator = CalculatorWithStubs();
            IAddress addressStub = Mock.Of<IAddress>(
                address => address.Country == "US" && address.City == "Test" && address.State == "MD" && address.Zip == null && address.StreetAddress == "123 E Main St"
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
        [DataRow("A")]
        [DataRow("ABC")]
        public async Task GetTaxRate_WrongStateLength_ThrowsServiceInputException(string invalidState)
        {
            TaxJarCalculator calculator = CalculatorWithStubs();
            IAddress addressStub = Mock.Of<IAddress>(
                address => address.Country == "US" && address.City == "Test" && address.State == invalidState && address.Zip == "21045" && address.StreetAddress == "123 E Main St"
            );

            await Assert.ThrowsExceptionAsync<ServiceInputException>(() => calculator.GetTaxRate(addressStub));
        }

        [TestMethod()]
        public void GetTaxRate_JsonConverter_SerializationError_ThrowsServiceInternalException()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetTaxRate_JsonConverter_DeserializationError_ThrowsServiceInternalException()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetTaxRate_RestHttpClient_RequestException_ThrowsServiceInternalException()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetTaxRate_AuthUnsuccessfulResponse_ThrowsServiceConfigurationException()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetTaxRate_ErrorResponse_ThrowsServiceInternalException()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetTaxRate_OtherInvalidResponse_ThrowsServiceInternalException()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetTaxRate_UknownResponseCountry_ThrowsServiceInternalException()
        {
            Assert.Fail();
        }

        [TestMethod()]
        [DataRow("")]
        [DataRow(null)]
        public void GetTaxRate_ZipOnly_UsTaxDecimal(string? otherElements)
        {
            Assert.Fail();
        }


        [TestMethod()]
        public void GetTaxRate_NonEuAddress_CorrectDecimal()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetTaxRate_EuAddress_CorrectDecimal()
        {
            Assert.Fail();
        }
        #endregion

        #region CalculateTaxes
        [TestMethod()]
        public void CalculateTaxes_NullToAddress_ThrowsServiceInputException()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void CalculateTaxes_ToUsNoZip_ThrowsServiceInputExcption()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void CalculateTaxes_ToUsOrCaNoState_ThrowsServiceInputException()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void CalculateTaxes_ToUnsupportedCountry_ThrowsServiceInputException()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void CalculateTaxes_FromUnsupportedCountry_ThrowsServiceInputException()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void CalculateTaxes_ToInvalidCountry_ThrowsServiceInputException()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void CalculateTaxes_FromInvalidCountry_ThrowsServiceInputException()
        {
            Assert.Fail();
        }


        [TestMethod()]
        public void CalculateTaxes_WrongToStateLength_ThrowsServiceInputException()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void CalculateTaxes_WrongFromStateLength_ThrowsServiceInputException()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void CalculateTaxes_JsonConverter_SerializationError_ThrowsServiceInternalException()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void CalculateTaxes_JsonConverter_DeserializationError_ThrowsServiceInternalException()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void CalculateTaxes_RestHttpClient_RequestException_ThrowsServiceInternalException()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void CalculateTaxes_AuthUnsuccessfulResponse_ThrowsServiceConfigurationException()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void CalculateTaxes_ErrorResponse_ThrowsServiceInternalException()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void CalculateTaxes_OtherInvalidResponse_ThrowsServiceInternalException()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void CalculateTaxes_UknownResponseCountry_ThrowsServiceInternalException()
        {
            Assert.Fail();
        }


        [TestMethod()]
        public void CalculateTaxes_NonEuAddress_CorrectDecimal()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void CalculateTaxes_EuAddress_CorrectDecimal()
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
        [DynamicData(nameof(InvalidCountryCodesDataIncludeBlank), DynamicDataSourceType.Method)]
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
        [DynamicData(nameof(InvalidCountryCodesDataIncludeBlank), DynamicDataSourceType.Method)]
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