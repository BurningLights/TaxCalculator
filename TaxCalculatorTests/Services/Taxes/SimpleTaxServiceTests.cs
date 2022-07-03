using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaxCalculator.Services.Taxes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaxCalculator.Data;
using Moq;
using TaxCalculator.Services.TaxCalculators;

namespace TaxCalculator.Services.Taxes.Tests
{
    [TestClass()]
    public class SimpleTaxServiceTests
    {
        private static ICustomer customerStub = Mock.Of<ICustomer>();

        [TestMethod()]
        [DataRow(false, true, 15, 1.5, 1.35)]
        [DataRow(true, false, 10, 0, 0.9)]
        public async Task CalculateTaxes_CorrectDecimal(bool includeCustomer, bool includeFromAddress, double orderAmount, double shipping, double taxAmount)
        {
            IAddress? fromAddress = includeFromAddress ? Mock.Of<IAddress>(
                address => address.StreetAddress == "9500 Gilman Drive" && address.City == "La Jolla" && address.State == "CA" && address.Zip == "92093" && address.Country == "US"
            ) : null;
            IAddress toAddress = Mock.Of<IAddress>(
                address => address.StreetAddress == "1335 E 103rd St" && address.City == "Los Angeles" && address.State == "CA" && address.Zip == "90002" && address.Country == "US"
            );
            decimal orderAmountm = Convert.ToDecimal(orderAmount);
            decimal shippingm = Convert.ToDecimal(shipping);
            decimal taxAmountm = Convert.ToDecimal(taxAmount);
            ITaxCalculator calculator = Mock.Of<ITaxCalculator>(calc => calc.CalculateTaxes(fromAddress, toAddress, orderAmountm, shippingm).Result == taxAmountm);

            SimpleTaxService taxService = new(calculator);

            Assert.AreEqual(taxAmountm, await taxService.CalculateTaxes(fromAddress, toAddress, orderAmountm, shippingm, includeCustomer ? customerStub : null));
        }

        [TestMethod()]
        public async Task CalculateTaxes_CalculatorThrowsServiceConfigurationException()
        {
            IAddress fromAddress = Mock.Of<IAddress>();
            IAddress toAddress = Mock.Of<IAddress>();

            Mock<ITaxCalculator> calculatorMock = new();
            calculatorMock.Setup(calc => calc.CalculateTaxes(It.IsAny<IAddress?>(), It.IsAny<IAddress>(), It.IsAny<decimal>(), It.IsAny<decimal>())).ThrowsAsync(new ServiceConfigurationException());

            SimpleTaxService taxService = new(calculatorMock.Object);

            await Assert.ThrowsExceptionAsync<ServiceConfigurationException>(() => taxService.CalculateTaxes(fromAddress, toAddress, 10m, 5m));
        }

        [TestMethod()]
        public async Task CalculateTaxes_CalculatorThrowsServiceInputException()
        {
            IAddress? fromAddress = null;
            IAddress toAddress = Mock.Of<IAddress>();

            Mock<ITaxCalculator> calculatorMock = new();
            calculatorMock.Setup(calc => calc.CalculateTaxes(It.IsAny<IAddress?>(), It.IsAny<IAddress>(), It.IsAny<decimal>(), It.IsAny<decimal>())).ThrowsAsync(new ServiceInputException());

            SimpleTaxService taxService = new(calculatorMock.Object);

            await Assert.ThrowsExceptionAsync<ServiceInputException>(() => taxService.CalculateTaxes(fromAddress, toAddress, 10m, 5m));
        }

        [TestMethod()]
        public async Task CalculateTaxes_CalculatorThrowsServiceInternalException()
        {
            IAddress? fromAddress = null;
            IAddress toAddress = Mock.Of<IAddress>();

            Mock<ITaxCalculator> calculatorMock = new();
            calculatorMock.Setup(calc => calc.CalculateTaxes(It.IsAny<IAddress?>(), It.IsAny<IAddress>(), It.IsAny<decimal>(), It.IsAny<decimal>())).ThrowsAsync(new ServiceInternalException());

            SimpleTaxService taxService = new(calculatorMock.Object);

            await Assert.ThrowsExceptionAsync<ServiceInternalException>(() => taxService.CalculateTaxes(fromAddress, toAddress, 10m, 5m));
        }

        [TestMethod()]
        [DataRow(true, 0.05)]
        [DataRow(false, 0.09)]

        public async Task GetTaxRate_CorrectDecimal(bool includeCustomer, double taxRate)
        {
            IAddress address = Mock.Of<IAddress>(
                address => address.StreetAddress == "1335 E 103rd St" && address.City == "Los Angeles" && address.State == "CA" && address.Zip == "90002" && address.Country == "US"
            );
            decimal taxRatem = Convert.ToDecimal(taxRate);
            ITaxCalculator calculator = Mock.Of<ITaxCalculator>(calc => calc.GetTaxRate(address).Result == taxRatem);

            SimpleTaxService taxService = new(calculator);

            Assert.AreEqual(taxRatem, await taxService.GetTaxRate(address, includeCustomer ? customerStub : null));
        }

        [TestMethod()]
        public async Task GetTaxRate_CalculatorThrowsServiceConfigurationException()
        {
            IAddress address = Mock.Of<IAddress>();

            Mock<ITaxCalculator> calculatorMock = new();
            calculatorMock.Setup(calc => calc.GetTaxRate(It.IsAny<IAddress>())).ThrowsAsync(new ServiceConfigurationException());

            SimpleTaxService taxService = new(calculatorMock.Object);

            await Assert.ThrowsExceptionAsync<ServiceConfigurationException>(() => taxService.GetTaxRate(address));
        }

        [TestMethod()]
        public async Task GetTaxRate_CalculatorThrowsServiceInputException()
        {
            IAddress address = Mock.Of<IAddress>();

            Mock<ITaxCalculator> calculatorMock = new();
            calculatorMock.Setup(calc => calc.GetTaxRate(It.IsAny<IAddress>())).ThrowsAsync(new ServiceInputException());

            SimpleTaxService taxService = new(calculatorMock.Object);

            await Assert.ThrowsExceptionAsync<ServiceInputException>(() => taxService.GetTaxRate(address));
        }

        [TestMethod()]
        public async Task GetTaxRate_CalculatorThrowsServiceInternalException()
        {
            IAddress address = Mock.Of<IAddress>();

            Mock<ITaxCalculator> calculatorMock = new();
            calculatorMock.Setup(calc => calc.GetTaxRate(It.IsAny<IAddress>())).ThrowsAsync(new ServiceInternalException());

            SimpleTaxService taxService = new(calculatorMock.Object);

            await Assert.ThrowsExceptionAsync<ServiceInternalException>(() => taxService.GetTaxRate(address));
        }

        [TestMethod()]
        [DataRow(new string[] { "US", "CA", "UK"})]
        [DataRow(new string[] { "AU", "IT" })]
        public void SupportedCountriesTest(string[] countries)
        {
            ITaxCalculator calculator = Mock.Of<ITaxCalculator>(calc => calc.SupportedCountries() == countries);
            Assert.AreEqual(countries, calculator.SupportedCountries());
        }
    }
}