using System;
using System.Collections.Generic;
using System.Text;

namespace TaxCalculator.Data
{
    internal interface IAddress
    {
        string Country { get; }
        string State { get; }
        string City { get; }
        string Zip { get; }
        string StreetAddress { get; }
    }
}
