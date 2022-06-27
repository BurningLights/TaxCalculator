using System;

namespace TaxCalculator.Rest
{

    [Serializable]
    public class RequestSendFailedException : RequestException
    {
        public RequestSendFailedException() { }
        public RequestSendFailedException(string message) : base(message) { }
        public RequestSendFailedException(string message, Exception inner) : base(message, inner) { }
        protected RequestSendFailedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
