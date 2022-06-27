using System;
using System.Collections.Generic;
using System.Text;

namespace TaxCalculator.Services
{

    [Serializable]
    public class ServiceConfigurationException : ServiceException
    {
        public ServiceConfigurationException() { }
        public ServiceConfigurationException(string message) : base(message) { }
        public ServiceConfigurationException(string message, Exception inner) : base(message, inner) { }
        protected ServiceConfigurationException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
