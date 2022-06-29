using System;
using System.Collections.Generic;
using System.Text;
using TaxCalculator.Data;
using System.Runtime.Serialization;

namespace TaxCalculator.Services.TaxCalculators.TaxJar.Responses
{
    public class RatesResponseWrapper
    {
        [DataMember(Name = "rate")]
        public RatesResponse Rate { get; set; } = new RatesResponse();  
    }

    [DataContract]
    public class RatesResponse : IAddress
    {
        [DataMember(Name = "country")]
        public string? Country { get; set; }

        [DataMember(Name = "name")]
        public string? CountryName { get; set; }

        [DataMember(Name = "country_rate")]
        public decimal CountryRate { get; set; }

        [DataMember(Name = "state")]
        public string? State { get; set; }

        [DataMember(Name = "state_rate")]
        public decimal StateRate { get; set; }

        [DataMember(Name = "county")]
        public string? County { get; set; }

        [DataMember(Name = "county_rate")]
        public decimal CountyRate { get; set; }

        [DataMember(Name = "city")]
        public string? City { get; set; }

        [DataMember(Name = "city_rate")]
        public decimal CityRate { get; set; }

        [DataMember(Name = "combined_district_rate")]
        public decimal TotalDistrictRate { get; set; }

        [DataMember(Name = "zip")]
        public string? Zip { get; set; }

        [DataMember(Name = "combined_rate")]
        public decimal TotalTaxRate { get; set; }

        [DataMember(Name = "freight_taxable")]
        public bool FreightTaxable { get; set; }

        public string StreetAddress => "";

        [DataMember(Name = "standard_rate")]
        public decimal StandardRate { get; set; }

        [DataMember(Name = "reduced_rate")]
        public decimal ReducedRate { get; set; }

        [DataMember(Name = "super_reduced_rate")]
        public decimal SuperReducedRate { get; set; }

        [DataMember(Name = "parking_rate")]
        public decimal ParkingRate { get; set; }

        [DataMember(Name = "distance_sale_threshold")]
        public decimal DistanceSaleThreshold { get; set; }
    }
}
