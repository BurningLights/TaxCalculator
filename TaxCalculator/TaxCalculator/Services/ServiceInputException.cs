using System;
using System.Collections.Generic;
using System.Text;

namespace TaxCalculator.Services
{

    [Serializable]
    public class ServiceInputException : ServiceException
    {
        public ServiceInputException() { }
        public ServiceInputException(string message) : base(message) { }
        public ServiceInputException(string message, Exception inner) : base(message, inner) { }
        protected ServiceInputException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
