using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace TaxCalculator.Services.TaxCalculators.TaxJar.Responses
{
    internal class FullTaxBreakdown : TaxBreakdownBase
    {
        [JsonProperty("shipping")]
        public TaxBreakdownBase ShippingBreakdown { get; set; }

        [JsonProperty("line_items")]
        public List<TaxBreakdownLineItem> LineItemBreakdowns { get; set; }
    }
}
