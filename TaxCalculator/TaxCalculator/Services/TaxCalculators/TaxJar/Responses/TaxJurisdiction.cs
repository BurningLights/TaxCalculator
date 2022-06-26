using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace TaxCalculator.Services.TaxCalculators.TaxJar.Responses
{
    internal class TaxJurisdiction
    {
        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("county")]
        public string County { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }
    }
}
