using System;
using System.Collections.Generic;
using System.Text;

namespace TaxCalculator.Services
{

    [Serializable]
    public class ServiceOutputException : Exception
    {
        public ServiceOutputException() { }
        public ServiceOutputException(string message) : base(message) { }
        public ServiceOutputException(string message, Exception inner) : base(message, inner) { }
        protected ServiceOutputException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
