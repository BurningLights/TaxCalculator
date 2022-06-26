using System;
using System.Collections.Generic;
using System.Text;

namespace TaxCalculator.Services.TaxCalculators
{

    [Serializable]
    public class IncompleteAddressException : Exception
    {
        public IncompleteAddressException() { }
        public IncompleteAddressException(string message) : base(message) { }
        public IncompleteAddressException(string message, Exception inner) : base(message, inner) { }
        protected IncompleteAddressException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
