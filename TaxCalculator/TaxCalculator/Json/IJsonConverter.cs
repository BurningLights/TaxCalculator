using System;
using System.Collections.Generic;
using System.Text;

namespace TaxCalculator.Json
{
    internal interface IJsonConverter
    {
        string SerializeObject(object obj);
        T DeserializeObject<T>(string json);
    }
}
