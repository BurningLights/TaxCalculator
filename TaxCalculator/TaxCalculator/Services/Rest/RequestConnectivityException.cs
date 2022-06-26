using System;
using System.Collections.Generic;
using System.Text;

namespace TaxCalculator.Services.Rest
{

    [Serializable]
    public class RequestConnectivityException : Exception
    {
        public RequestConnectivityException() { }
        public RequestConnectivityException(string message) : base(message) { }
        public RequestConnectivityException(string message, Exception inner) : base(message, inner) { }
        protected RequestConnectivityException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
