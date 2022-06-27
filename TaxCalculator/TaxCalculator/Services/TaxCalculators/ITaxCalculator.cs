using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TaxCalculator.Data;

namespace TaxCalculator.Services.TaxCalculators
{
    internal interface ITaxCalculator
    {
        Task<decimal> GetTaxRate(IAddress address);
        Task<decimal> CalculateTaxes(IAddress fromAddress, IAddress toAddress, decimal amount, decimal shipping);
        IEnumerable<string> SupportedCountries();
    }
}
