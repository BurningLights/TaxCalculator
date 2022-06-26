using System;
using System.Collections.Generic;
using System.Text;

namespace TaxCalculator.Services.Rest
{

    [Serializable]
    public class RequestSendFailedException : Exception
    {
        public RequestSendFailedException() { }
        public RequestSendFailedException(string message) : base(message) { }
        public RequestSendFailedException(string message, Exception inner) : base(message, inner) { }
        protected RequestSendFailedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
