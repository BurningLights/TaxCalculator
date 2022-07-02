using System.Runtime.Serialization;

namespace TaxCalculator.Services.TaxCalculators.TaxJar.Responses
{
    [DataContract]
    public class TaxBreakdownBase
    {
        [DataMember(Name = "taxable_amount")]
        public decimal TaxableAmount { get; set; }

        [DataMember(Name = "tax_collectable")]
        public decimal TaxCollectable { get; set; }

        [DataMember(Name = "combined_tax_rate")]
        public decimal CombinedTaxRate { get; set; }

        [DataMember(Name = "state_taxable_amount")]
        public decimal StateTaxableAmount { get; set; }

        [DataMember(Name = "state_tax_rate")]
        public decimal StateTaxRate { get; set; }

        [DataMember(Name = "state_tax_collectable")]
        public decimal StateTaxCollectable { get; set; }

        [DataMember(Name = "county_taxable_amount")]
        public decimal CountyTaxableAmount { get; set; }

        [DataMember(Name = "county_tax_rate")]
        public decimal CountyTaxRate { get; set; }

        [DataMember(Name = "county_tax_collectable")]
        public decimal CountyTaxCollectable { get; set; }

        [DataMember(Name = "city_taxable_amount")]
        public decimal CityTaxableAmount { get; set; }

        [DataMember(Name = "city_tax_rate")]
        public decimal CityTaxRate { get; set; }

        [DataMember(Name = "city_tax_collectable")]
        public decimal CityTaxCollectable { get; set; }

        [DataMember(Name = "special_district_taxable_amount")]
        public decimal SpecialDistrictTaxableAmount { get; set; }

        [DataMember(Name = "special_tax_rate")]
        public decimal SpecialTaxRate { get; set; }

        [DataMember(Name = "special_district_tax_collectable")]
        public decimal SpecialDistrictTaxCollectable { get; set; }

        [DataMember(Name = "gst_taxable_amount")]
        public decimal GstTaxableAmount { get; set; }

        [DataMember(Name = "gst_tax_rate")]
        public decimal GstTaxRate { get; set; }

        [DataMember(Name = "gst")]
        public decimal Gst { get; set; }

        [DataMember(Name = "pst_taxable_amount")]
        public decimal PstTaxableAmount { get; set; }

        [DataMember(Name = "pst_tax_rate")]
        public decimal PstTaxRate { get; set; }

        [DataMember(Name = "pst")]
        public decimal Pst { get; set; }

        [DataMember(Name = "qst_taxable_amount")]
        public decimal QstTaxableAmount { get; set; }

        [DataMember(Name = "qst_tax_rate")]
        public decimal QstTaxRate { get; set; }

        [DataMember(Name = "qst")]
        public decimal Qst { get; set; }

        [DataMember(Name = "country_taxable_amount")]
        public decimal CountryTaxableAmount { get; set; }

        [DataMember(Name = "country_tax_rate")]
        public decimal CountryTaxRate { get; set; }

        [DataMember(Name = "country_tax_collectable")]
        public decimal CountryTaxCollectable { get; set; }
    }
}
