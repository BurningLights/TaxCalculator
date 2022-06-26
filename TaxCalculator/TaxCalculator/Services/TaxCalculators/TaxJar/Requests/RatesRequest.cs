using System;
using System.Collections.Generic;
using System.Text;
using TaxCalculator.Data;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace TaxCalculator.Services.TaxCalculators.TaxJar.Requests
{
    [DataContract]
    internal class RatesRequest : IAddress
    {
        [DataMember(Name = "country", EmitDefaultValue = false)]
        [DefaultValue("US")]
        public string Country { get; set; }

        [DataMember(Name = "state", EmitDefaultValue = false)]
        public string State { get; set; }

        [DataMember(Name = "city", EmitDefaultValue = false)]
        public string City { get; set; }

        [DataMember(Name ="street", EmitDefaultValue = false)]
        public string StreetAddress { get; set; }

        public string Zip { get; set; }

        public RatesRequest(string zip) => Zip = zip;


        public RatesRequest(string zip, string country)
        {
            Zip = zip;
            Country = country;
        }

        public RatesRequest(string zip, string country, string state, string city, string streetAddress)
        {
            Zip = zip;
            Country = country;
            State = state;
            City = city;
            StreetAddress = streetAddress;
        }

        public RatesRequest(IAddress address) : this(address.Zip, address.Country, address.State, address.City, address.StreetAddress) { }
    }
}
