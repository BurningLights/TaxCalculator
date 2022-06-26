using System;
using System.Collections.Generic;
using System.Text;
using TaxCalculator.Data;
using Newtonsoft.Json;
using System.ComponentModel;

namespace TaxCalculator.Services.TaxCalculators.TaxJar.Requests
{
    internal class RatesRequest : IAddress
    {
        [JsonProperty("country", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue("US")]
        public string Country { get; set; }

        [JsonProperty("state", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string State { get; set; }

        [JsonProperty("city", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string City { get; set; }

        [JsonProperty("zip", Required = Required.Always)]
        public string Zip { get; set; }

        [JsonProperty("street", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string StreetAddress { get; set; }

        public RatesRequest() { };

        public RatesRequest(string zip)
        {
            Zip = zip;
        }

        public RatesRequest(string zip, string country)
        {
            Zip = zip;
            Country = country;
        }

        public RatesRequest(string zip, string country, string state, string city, string streetAddress)
        {
            Country = country;
            State = state;
            City = city;
            Zip = zip;
            StreetAddress = streetAddress;
        }

        public RatesRequest(IAddress address) : this(address.Zip, address.Country, address.State, address.City, address.StreetAddress) { }
    }
}
