using System.Runtime.Serialization;

namespace TaxCalculator.Services.TaxCalculators.TaxJar.Responses
{
    internal class ErrorResponse
    {
        [DataMember(Name = "error")]
        public string ErrorName { get; set; }

        [DataMember(Name = "detail")]
        public string Detail { get; set; }

        [DataMember(Name = "status")]
        public string Status { get; set; }
    }
}
