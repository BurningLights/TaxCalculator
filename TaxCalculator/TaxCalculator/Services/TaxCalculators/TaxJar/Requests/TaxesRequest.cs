using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using TaxCalculator.Data;

namespace TaxCalculator.Services.TaxCalculators.TaxJar.Requests
{
    internal class TaxesRequest
    {
        [JsonProperty("from_country", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string FromCountry { get; set; }

        [JsonProperty("from_zip", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string FromZip { get; set; }

        [JsonProperty("from_state", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string FromState { get; set; }

        [JsonProperty("from_city", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string FromCity { get; set; }

        [JsonProperty("from_street", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string FromStreet { get; set; }

        [JsonProperty("to_country", Required = Required.Always)]
        public string ToCountry { get; set; }

        [JsonProperty("to_zip", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ToZip { get; set; }

        [JsonProperty("to_state", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ToState { get; set; }

        [JsonProperty("to_city", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ToCity { get; set; }

        [JsonProperty("to_street", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ToStreet { get; set; }

        [JsonProperty("amount", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public decimal Amount { get; set; }

        [JsonProperty("shipping", Required = Required.Always)]
        public decimal Shipping { get; set; }

        [JsonProperty("customer_id", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string CustomerId { get; set; }

        [JsonProperty("exemption_type", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ExemptionType { get; set; }

        public TaxesRequest() { }

        public TaxesRequest(string toCountry, decimal shipping) 
        {
            ToCountry = toCountry;
            Shipping = shipping;
        }

        public TaxesRequest(IAddress toAddress, IAddress fromAddress, decimal shipping, decimal amount) 
        {
            ToCountry = toAddress.Country;
            ToState = toAddress.State;
            ToCity = toAddress.City;
            ToZip = toAddress.Zip;
            FromCountry = fromAddress.Country;
            FromState = fromAddress.State;
            FromCity = fromAddress.City;
            FromZip = fromAddress.Zip;
            Shipping = shipping;
            Amount = amount;
        }
    }
}
