using System.Runtime.Serialization;

namespace TaxCalculator.Services.TaxCalculators.TaxJar.Responses
{
    internal class TaxJurisdiction
    {
        [DataMember(Name = "country")]
        public string Country { get; set; }

        [DataMember(Name = "state")]
        public string State { get; set; }

        [DataMember(Name = "county")]
        public string County { get; set; }

        [DataMember(Name = "city")]
        public string City { get; set; }
    }
}
