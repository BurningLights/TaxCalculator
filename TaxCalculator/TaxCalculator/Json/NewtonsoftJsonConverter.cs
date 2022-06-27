using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using TaxCalculator.Services.Json;

namespace TaxCalculator.Json
{
    internal class NewtonsoftJsonConverter : IJsonConverter
    {
        public T DeserializeObject<T>(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (JsonException ex)
            {
                throw new DeserializationException("Could not deserialize JSON to object", ex);
            }
        }
        public string SerializeObject(object obj)
        {
            try
            {
                return JsonConvert.SerializeObject(obj);
            }
            catch (JsonException ex)
            {
                throw new SerializationException("Could not serialize object to JSON string", ex);
            }
        }
    }
}
