using System;
using System.Collections.Generic;
using System.Text;

namespace TaxCalculator.Data
{
    public interface IAddress
    {
        string Country { get; }
        string State { get; }
        string City { get; }
        string Zip { get; }
        string StreetAddress { get; }
    }
}
