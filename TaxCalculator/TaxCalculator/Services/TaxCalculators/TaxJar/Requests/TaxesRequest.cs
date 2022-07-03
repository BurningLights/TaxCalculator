using System;
using System.Collections.Generic;
using System.Text;
using TaxCalculator.Data;
using System.Runtime.Serialization;

namespace TaxCalculator.Services.TaxCalculators.TaxJar.Requests
{
    [DataContract]
    public class TaxesRequest
    {
        private string? fromCountry;
        private string? fromZip;
        private string? fromState;
        private string? fromCity;
        private string? fromStreet;
        private string? toZip;
        private string? toState;
        private string? toCity;
        private string? toStreet;

        private static string? BlankToNull(string? value) => string.IsNullOrEmpty(value) ? null : value;

        [DataMember(Name = "from_country", EmitDefaultValue = false)]
        public string? FromCountry { get => fromCountry; set => fromCountry = BlankToNull(value); }

        [DataMember(Name = "from_zip", EmitDefaultValue = false)]
        public string? FromZip { get => fromZip; set => fromZip = BlankToNull(value); }

        [DataMember(Name = "from_state", EmitDefaultValue = false)]
        public string? FromState { get => fromState; set => fromState = BlankToNull(value); }

        [DataMember(Name = "from_city", EmitDefaultValue = false)]
        public string? FromCity { get => fromCity; set => fromCity = BlankToNull(value); }

        [DataMember(Name = "from_street", EmitDefaultValue = false)]
        public string? FromStreet { get => fromStreet; set => fromStreet = BlankToNull(value); }

        [DataMember(Name = "to_country", IsRequired = true)]
        public string ToCountry { get; set; } = "";

        [DataMember(Name = "to_zip", EmitDefaultValue = false)]
        public string? ToZip { get => toZip; set => toZip = BlankToNull(value); }

        [DataMember(Name = "to_state", EmitDefaultValue = false)]
        public string? ToState { get => toState; set => toState = BlankToNull(value); }

        [DataMember(Name = "to_city", EmitDefaultValue = false)]
        public string? ToCity { get => toCity; set => toCity = BlankToNull(value); }

        [DataMember(Name = "to_street", EmitDefaultValue = false)]
        public string? ToStreet { get => toStreet; set => toStreet = BlankToNull(value); }

        [DataMember(Name = "amount", EmitDefaultValue = false)]
        public decimal Amount { get; set; }

        [DataMember(Name = "shipping", IsRequired = true)]
        public decimal Shipping { get; set; }

        public TaxesRequest() { }

        public TaxesRequest(string toCountry, decimal shipping) 
        {
            ToCountry = toCountry;
            Shipping = shipping;
        }

        public TaxesRequest(IAddress? fromAddress, IAddress toAddress, decimal amount, decimal shipping) 
        {
            ToCountry = toAddress.Country ?? "";
            ToState = toAddress.State;
            ToCity = toAddress.City;
            ToZip = toAddress.Zip;
            ToStreet = toAddress.StreetAddress;
            FromCountry = fromAddress?.Country;
            FromState = fromAddress?.State;
            FromCity = fromAddress?.City;
            FromZip = fromAddress?.Zip;
            FromStreet = fromAddress?.StreetAddress;
            Shipping = shipping;
            Amount = amount;
        }
    }
}
