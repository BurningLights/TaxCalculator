using System;

namespace TaxCalculator.Json
{

    [Serializable]
    public class SerializationException : JsonConversionException
    {
        public SerializationException() { }
        public SerializationException(string message) : base(message) { }
        public SerializationException(string message, Exception inner) : base(message, inner) { }
        protected SerializationException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
