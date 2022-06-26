using System.Runtime.Serialization;

namespace TaxCalculator.Services.TaxCalculators.TaxJar.Responses
{
    internal class TaxesResponseWrapper
    {
        [DataMember(Name = "tax")]
        public TaxesResponse Tax { get; set; }
    }
    internal class TaxesResponse
    {
        [DataMember(Name = "order_total_amount")]
        public decimal OrderTotal { get; set; }

        [DataMember(Name = "shipping")]
        public decimal Shipping { get; set; }

        [DataMember(Name = "taxable_amount")]
        public decimal TaxableAmount { get; set; }

        [DataMember(Name = "amount_to_collect")]
        public decimal TaxToCollect { get; set; }

        [DataMember(Name = "rate")]
        public decimal TaxRate { get; set; }

        [DataMember(Name = "has_nexus")]
        public decimal HasNexus { get; set; }

        [DataMember(Name = "freight_taxable")]
        public decimal FreightTaxable { get; set; }

        [DataMember(Name = "tax_source")]
        public string TaxSource { get; set; }

        [DataMember(Name = "exemption_type")]
        public string ExemptionType { get; set; }

        [DataMember(Name = "jurisdications")]
        public TaxJurisdiction Jurisdictions { get; set; }

        [DataMember(Name = "breakdown")]
        public FullTaxBreakdown Breakdown { get; set; }
    }
}
