using System;
using System.Collections.Generic;
using System.Text;
using TaxCalculator.Data;

namespace TaxCalculator.Services.Taxes
{
    internal interface ITaxService
    {
        decimal GetTaxRate(IAddress address, ICustomer customer = null);
        decimal CalculateTaxes(IAddress fromAddress, IAddress toAddress, decimal amount, decimal shipping, ICustomer customer = null);

        IList<string> SupportedCountries(ICustomer customer = null);
    }
}
