using System;
using System.Collections.Generic;
using System.Text;
using TaxCalculator.Data;

namespace TaxCalculator.Services.TaxCalculators
{
    internal interface ITaxCalculator
    {
        decimal GetTaxRate(IAddress address);
        decimal CalculateTaxes(IAddress fromAddress, IAddress toAddress, decimal amount, decimal shipping);
        IList<string> SupportedCountries();
    }
}
