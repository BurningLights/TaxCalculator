using System;

namespace TaxCalculator.Rest
{

    [Serializable]
    public class RequestTimeoutException : RequestException
    {
        public RequestTimeoutException() { }
        public RequestTimeoutException(string message) : base(message) { }
        public RequestTimeoutException(string message, Exception inner) : base(message, inner) { }
        protected RequestTimeoutException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
