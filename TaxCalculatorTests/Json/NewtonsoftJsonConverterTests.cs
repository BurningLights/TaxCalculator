using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaxCalculator.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaxCalculator.Json.Tests
{
    public class SimpleObject
    {
        public string? Item { get; set; }
    }

    public class CircularClass
    {
        public CircularClass? Item { get; set; }
    }

    [TestClass()]
    public class NewtonsoftJsonConverterTests
    {
        private static IEnumerable<object?[]> TestData { get; } = new object?[][]
        {
            new object?[] { "null", null },
            new object?[] { "{\"Item\":\"test\"}", new SimpleObject() { Item = "test" } },
            new object?[] { "{\"Item\":null}", new SimpleObject() { Item = null } },
        };

        [TestMethod()]
        [DynamicData(nameof(TestData))]
        public void DeserializeObjectTest(string text, SimpleObject? obj)
        {
            NewtonsoftJsonConverter jsonConverter = new();
            SimpleObject? result = jsonConverter.DeserializeObject<SimpleObject>(text);
            if (obj == null)
            {
                Assert.IsNull(result);
            }
            else
            {
                Assert.IsNotNull(result);
                Assert.AreEqual(obj.Item, result.Item);
            }
        }

        [TestMethod]
        public void Deserialize_JsonException_ThrowsDeserializationException()
        {
            NewtonsoftJsonConverter jsonConverter = new();

            Assert.ThrowsException<DeserializationException>(() => jsonConverter.DeserializeObject<SimpleObject>("{"));
        }

        [TestMethod()]
        [DynamicData(nameof(TestData))]
        public void SerializeObjectTest(string text, SimpleObject? obj)
        {
            NewtonsoftJsonConverter jsonConverter = new();
            string result = jsonConverter.SerializeObject(obj);
            Assert.AreEqual(text, result);
        }

        [TestMethod]
        public void Deserialize_JsonException_ThrowsSerializationException()
        {
            NewtonsoftJsonConverter jsonConverter = new();

            CircularClass circularClass = new();
            circularClass.Item = circularClass;

            Assert.ThrowsException<SerializationException>(() => jsonConverter.SerializeObject(circularClass));
        }
    }
}