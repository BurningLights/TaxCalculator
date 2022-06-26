using System;
using System.Collections.Generic;
using System.Text;
using TaxCalculator.Data;
using TaxCalculator.Services.TaxCalculators;

namespace TaxCalculator.Services.Taxes
{
    internal class SimpleTaxService : ITaxService
    {
        private ITaxCalculator taxCalculator;

        public SimpleTaxService(ITaxCalculator taxCalculator) => this.taxCalculator = taxCalculator;

        public decimal CalculateTaxes(IAddress fromAddress, IAddress toAddress, decimal amount, decimal shipping, ICustomer customer = null)
        {
            return taxCalculator.CalculateTaxes(fromAddress, toAddress, amount);
        }
        public decimal GetTaxRate(IAddress address, ICustomer customer = null)
        {
            return taxCalculator.GetTaxRate(address);
        }
    }
}
