using System.Collections.Generic;
using System.Threading.Tasks;
using TaxCalculator.Data;

namespace TaxCalculator.Services.Taxes
{
    internal interface ITaxService
    {
        Task<decimal> GetTaxRate(IAddress address, ICustomer customer = null);
        Task<decimal> CalculateTaxes(IAddress fromAddress, IAddress toAddress, decimal amount, decimal shipping, ICustomer customer = null);

        IEnumerable<string> SupportedCountries(ICustomer customer = null);
    }
}
