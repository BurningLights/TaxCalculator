using System;
using System.Collections.Generic;
using System.Text;
using TaxCalculator.Data;
using System.Runtime.Serialization;

namespace TaxCalculator.Services.TaxCalculators.TaxJar.Requests
{
    [DataContract]
    internal class TaxesRequest
    {
        [DataMember(Name = "from_country", EmitDefaultValue = false)]
        public string FromCountry { get; set; }

        [DataMember(Name = "from_zip", EmitDefaultValue = false)]
        public string FromZip { get; set; }

        [DataMember(Name = "from_state", EmitDefaultValue = false)]
        public string FromState { get; set; }

        [DataMember(Name = "from_city", EmitDefaultValue = false)]
        public string FromCity { get; set; }

        [DataMember(Name = "from_street", EmitDefaultValue = false)]
        public string FromStreet { get; set; }

        [DataMember(Name = "to_country", IsRequired = true)]
        public string ToCountry { get; set; }

        [DataMember(Name = "to_zip", EmitDefaultValue = false)]
        public string ToZip { get; set; }

        [DataMember(Name = "to_state", EmitDefaultValue = false)]
        public string ToState { get; set; }

        [DataMember(Name = "to_city", EmitDefaultValue = false)]
        public string ToCity { get; set; }

        [DataMember(Name = "to_street", EmitDefaultValue = false)]
        public string ToStreet { get; set; }

        [DataMember(Name = "amount", EmitDefaultValue = false)]
        public decimal Amount { get; set; }

        [DataMember(Name = "shipping", IsRequired = true)]
        public decimal Shipping { get; set; }

        [DataMember(Name = "customer_id", EmitDefaultValue = false)]
        public string CustomerId { get; set; }

        [DataMember(Name = "exemption_type", EmitDefaultValue = false)]
        public string ExemptionType { get; set; }

        public TaxesRequest() { }

        public TaxesRequest(string toCountry, decimal shipping) 
        {
            ToCountry = toCountry;
            Shipping = shipping;
        }

        public TaxesRequest(IAddress fromAddress, IAddress toAddress, decimal shipping, decimal amount) 
        {
            ToCountry = toAddress.Country;
            ToState = toAddress.State;
            ToCity = toAddress.City;
            ToZip = toAddress.Zip;
            FromCountry = fromAddress.Country;
            FromState = fromAddress.State;
            FromCity = fromAddress.City;
            FromZip = fromAddress.Zip;
            Shipping = shipping;
            Amount = amount;
        }
    }
}
