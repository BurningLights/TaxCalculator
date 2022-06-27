using System;
using System.Collections.Generic;
using System.Text;

namespace TaxCalculator.Json
{

    [Serializable]
    public class JsonConversionException : Exception
    {
        public JsonConversionException() { }
        public JsonConversionException(string message) : base(message) { }
        public JsonConversionException(string message, Exception inner) : base(message, inner) { }
        protected JsonConversionException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
