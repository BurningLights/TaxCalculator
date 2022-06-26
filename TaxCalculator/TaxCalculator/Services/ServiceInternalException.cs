using System;
using System.Collections.Generic;
using System.Text;

namespace TaxCalculator.Services
{

    [Serializable]
    public class ServiceInternalException : Exception
    {
        public ServiceInternalException() { }
        public ServiceInternalException(string message) : base(message) { }
        public ServiceInternalException(string message, Exception inner) : base(message, inner) { }
        protected ServiceInternalException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
