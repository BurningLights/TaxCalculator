using System.Runtime.Serialization;

namespace TaxCalculator.Services.TaxCalculators.TaxJar.Responses
{
    internal class TaxBreakdownLineItem : TaxBreakdownBase
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }
    }
}
