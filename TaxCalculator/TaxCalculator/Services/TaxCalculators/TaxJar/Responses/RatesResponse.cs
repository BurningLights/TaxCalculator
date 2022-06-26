using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using TaxCalculator.Data;

namespace TaxCalculator.Services.TaxCalculators.TaxJar.Responses
{
    internal class RatesResponseWrapper
    {
        [JsonProperty("rate")]
        public RatesResponse Rate { get; set; }
    }
    internal class RatesResponse : IAddress
    {
        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("name")]
        public string CountryName { get; set; }

        [JsonProperty("country_rate")]
        public decimal CountryRate { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("state_rate")]
        public decimal StateRate { get; set; }

        [JsonProperty("county")]
        public string County { get; set; }

        [JsonProperty("county_rate")]
        public decimal CountyRate { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("city_rate")]
        public decimal CityRate { get; set; }

        [JsonProperty("combined_district_rate")]
        public decimal TotalDistrictRate { get; set; }

        [JsonProperty("zip")]
        public string Zip { get; set; }

        [JsonProperty("combined_rate")]
        public decimal TotalTaxRate { get; set; }

        [JsonProperty("freight_taxable")]
        public bool FreightTaxable { get; set; }

        [JsonIgnore]
        public string StreetAddress => "";

        [JsonProperty("standard_rate")]
        public decimal StandardRate { get; set; }

        [JsonProperty("reduced_rate")]
        public decimal ReducedRate { get; set; }

        [JsonProperty("super_reduced_rate")]
        public decimal SuperReducedRate { get; set; }

        [JsonProperty("parking_rate")]
        public decimal ParkingRate { get; set; }

        [JsonProperty("distance_sale_threshold")]
        public decimal DistanceSaleThreshold { get; set; }
    }
}
