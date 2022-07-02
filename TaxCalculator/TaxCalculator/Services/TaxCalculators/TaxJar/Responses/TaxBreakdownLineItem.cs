using System.Runtime.Serialization;

namespace TaxCalculator.Services.TaxCalculators.TaxJar.Responses
{
    [DataContract]
    public class TaxBreakdownLineItem : TaxBreakdownBase
    {
        [DataMember(Name = "id")]
        public string Id { get; set; } = "";
    }
}
