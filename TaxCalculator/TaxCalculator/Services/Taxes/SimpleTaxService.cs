using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TaxCalculator.Data;
using TaxCalculator.Services.TaxCalculators;

namespace TaxCalculator.Services.Taxes
{
    internal class SimpleTaxService : ITaxService
    {
        private ITaxCalculator taxCalculator;

        public SimpleTaxService(ITaxCalculator taxCalculator) => this.taxCalculator = taxCalculator;

        public Task<decimal> CalculateTaxes(IAddress? fromAddress, IAddress toAddress, decimal amount, decimal shipping, ICustomer? customer = null) => taxCalculator.CalculateTaxes(fromAddress, toAddress, amount, shipping);
        public Task<decimal> GetTaxRate(IAddress address, ICustomer? customer = null) => taxCalculator.GetTaxRate(address);

        public IEnumerable<string> SupportedCountries(ICustomer? customer = null) => taxCalculator.SupportedCountries();
    }
}
