using System;
using System.Collections.Generic;
using System.Text;

namespace TaxCalculator.Json
{
    public interface IJsonConverter
    {
        string SerializeObject(object? obj);
        T? DeserializeObject<T>(string json) where T : class;
    }
}
