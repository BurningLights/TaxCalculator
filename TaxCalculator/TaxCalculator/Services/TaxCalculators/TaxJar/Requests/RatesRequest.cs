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

        [JsonProperty("street", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string StreetAddress { get; set; }

        public RatesRequest() { }


        public RatesRequest(string country)
        {
            Country = country;
        }

        public RatesRequest(string country, string state, string city, string streetAddress)
        {
            Country = country;
            State = state;
            City = city;
            StreetAddress = streetAddress;
        }

        public RatesRequest(IAddress address) : this(address.Country, address.State, address.City, address.StreetAddress) { }
    }
}
