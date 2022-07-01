using System;
using System.Collections.Generic;
using System.Text;
using TaxCalculator.Data;

namespace TaxCalculator.ViewModels
{
    internal class AddressViewModel : BaseViewModel, IAddress
    {
        private string? country;
        private string? state;
        private string? city;
        private string? zip;
        private string? streetAddress;

        public string? Country { get => country; set => SetPropertyValue(ref country, value); }

        public string? State { get => state; set => SetPropertyValue(ref state, value); }

        public string? City { get => city; set => SetPropertyValue(ref city, value); }

        public string? Zip { get => zip; set => SetPropertyValue(ref zip, value); }

        public string? StreetAddress { get => streetAddress; set => SetPropertyValue(ref streetAddress, value); }
    }
}
