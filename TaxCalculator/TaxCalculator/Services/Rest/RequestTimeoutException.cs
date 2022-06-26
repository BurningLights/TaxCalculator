using System;
using System.Collections.Generic;
using System.Text;

namespace TaxCalculator.Services.Rest
{

    [Serializable]
    public class RequestTimeoutException : Exception
    {
        public RequestTimeoutException() { }
        public RequestTimeoutException(string message) : base(message) { }
        public RequestTimeoutException(string message, Exception inner) : base(message, inner) { }
        protected RequestTimeoutException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
