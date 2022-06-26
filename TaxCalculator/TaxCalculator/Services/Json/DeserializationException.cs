using System;
using System.Collections.Generic;
using System.Text;

namespace TaxCalculator.Services.Json
{

    [Serializable]
    public class DeserializationException : Exception
    {
        public DeserializationException() { }
        public DeserializationException(string message) : base(message) { }
        public DeserializationException(string message, Exception inner) : base(message, inner) { }
        protected DeserializationException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
