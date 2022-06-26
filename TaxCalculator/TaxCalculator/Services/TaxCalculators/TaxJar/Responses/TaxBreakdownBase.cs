using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace TaxCalculator.Services.TaxCalculators.TaxJar.Responses
{
    internal class TaxBreakdownBase
    {
        [JsonProperty("taxable_amount")]
        public decimal TaxableAmount { get; set; }

        [JsonProperty("tax_collectable")]
        public decimal TaxCollectable { get; set; }

        [JsonProperty("combined_tax_rate")]
        public decimal CombinedTaxRate { get; set; }

        [JsonProperty("state_taxable_amount")]
        public decimal StateTaxableAmount { get; set; }

        [JsonProperty("state_tax_rate")]
        public decimal StateTaxRate { get; set; }

        [JsonProperty("state_tax_collectable")]
        public decimal StateTaxCollectable { get; set; }

        [JsonProperty("county_taxable_amount")]
        public decimal CountyTaxableAmount { get; set; }

        [JsonProperty("county_tax_rate")]
        public decimal CountyTaxRate { get; set; }

        [JsonProperty("county_tax_collectable")]
        public decimal CountyTaxCollectable { get; set; }

        [JsonProperty("city_taxable_amount")]
        public decimal CityTaxableAmount { get; set; }

        [JsonProperty("city_tax_rate")]
        public decimal CityTaxRate { get; set; }

        [JsonProperty("city_tax_collectable")]
        public decimal CityTaxCollectable { get; set; }

        [JsonProperty("special_district_taxable_amount")]
        public decimal SpecialDistrictTaxableAmount { get; set; }

        [JsonProperty("special_tax_rate")]
        public decimal SpecialTaxRate { get; set; }

        [JsonProperty("special_district_tax_collectable")]
        public decimal SpecialDistrictTaxCollectable { get; set; }

        [JsonProperty("gst_taxable_amount")]
        public decimal GstTaxableAmount { get; set; }

        [JsonProperty("gst_tax_rate")]
        public decimal GstTaxRate { get; set; }

        [JsonProperty("gst")]
        public decimal Gst { get; set; }

        [JsonProperty("pst_taxable_amount")]
        public decimal PstTaxableAmount { get; set; }

        [JsonProperty("pst_tax_rate")]
        public decimal PstTaxRate { get; set; }

        [JsonProperty("pst")]
        public decimal Pst { get; set; }

        [JsonProperty("qst_taxable_amount")]
        public decimal QstTaxableAmount { get; set; }

        [JsonProperty("qst_tax_rate")]
        public decimal QstTaxRate { get; set; }

        [JsonProperty("qst")]
        public decimal Qst { get; set; }

        [JsonProperty("country_taxable_amount")]
        public decimal CountryTaxableAmount { get; set; }

        [JsonProperty("country_tax_rate")]
        public decimal CountryTaxRate { get; set; }

        [JsonProperty("country_tax_collectable")]
        public decimal CountryTaxCollectable { get; set; }

        // TODO: Shipping and line items

    }
}
