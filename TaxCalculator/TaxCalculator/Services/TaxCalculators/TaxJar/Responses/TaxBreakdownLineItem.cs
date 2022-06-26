using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace TaxCalculator.Services.TaxCalculators.TaxJar.Responses
{
    internal class TaxBreakdownLineItem : TaxBreakdownBase
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}
