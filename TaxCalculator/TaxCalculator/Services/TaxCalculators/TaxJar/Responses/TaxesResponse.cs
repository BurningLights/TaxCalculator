using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace TaxCalculator.Services.TaxCalculators.TaxJar.Responses
{
    internal class TaxesResponseWrapper
    {
        [JsonProperty("tax")]
        public TaxesResponse Tax { get; set; }
    }
    internal class TaxesResponse
    {
        [JsonProperty("order_total_amount")]
        public decimal OrderTotal { get; set; }

        [JsonProperty("shipping")]
        public decimal Shipping { get; set; }

        [JsonProperty("taxable_amount")]
        public decimal TaxableAmount { get; set; }

        [JsonProperty("amount_to_collect")]
        public decimal TaxToCollect { get; set; }

        [JsonProperty("rate")]
        public decimal TaxRate { get; set; }

        [JsonProperty("has_nexus")]
        public decimal HasNexus { get; set; }

        [JsonProperty("freight_taxable")]
        public decimal FreightTaxable { get; set; }

        [JsonProperty("tax_source")]
        public string TaxSource { get; set; }

        [JsonProperty("exemption_type")]
        public string ExemptionType { get; set; }

        [JsonProperty("jurisdications")]
        public TaxJurisdiction Jurisdictions { get; set; }

        [JsonProperty("breakdown")]
        public FullTaxBreakdown Breakdown { get; set; }
    }
}
