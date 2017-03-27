using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TestCommon;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Xunit;
using Refit;

namespace Refit.Tests
{
    
    public class MultiFormDataDictionaryTests
    {
        private const string paramName = "test";

        public class ParentObj
        {
             public ChildObject Child { get; set; }

        }
        public class ChildObject
        {
            public int ChildId { get; set; }
            public string ChildName { get; set; }

        }

        public class DateData
        {
            public DateTimeOffset TestDate { get; set; }
        }

        private static DateTimeOffset d = new DateTimeOffset(2000, 12, 30, 12, 0, 0, TimeSpan.FromHours(8));
        public static TheoryDataSet<Tuple<object, KeyValuePair<string, string>>> ObjectData => new TheoryDataSet<Tuple<object, KeyValuePair<string, string>>>()
        {
            new Tuple<object, KeyValuePair<string, string>>(
                new Dictionary<string, string>()
                {
                    {"Item1", "hello"}
                },
                new KeyValuePair<string, string>($"Item1", "hello")

            ),
            new Tuple<object, KeyValuePair<string, string>>(
            new DateData()
            {
                TestDate = d
            },
            new KeyValuePair<string, string>($"{paramName}[testDate]", d.ToString("yyyy-MM-ddThh:mm:sszzzz"))
            ),
            new Tuple<object, KeyValuePair<string, string>>(
             new ParentObj()
            {
               Child = new ChildObject()
               {
                   ChildId = 1,
                   ChildName = "Hello"
               }
            },
              new KeyValuePair<string, string>($"{paramName}[child][childId]", $"{1}")
            ),
            new Tuple<object, KeyValuePair<string, string>>(
             new List<int> {1, 2, 3},
              new KeyValuePair<string, string>($"{paramName}[0]", $"{1}")
            )

            
        };

      

        [Theory]
        [TestDataSet(typeof(MultiFormDataDictionaryTests), nameof(ObjectData))]
        public void ShouldFlattenPropertiesAsKeyValuePair(Tuple<object, KeyValuePair<string, string>> data)
        {
            var r = new MultiFormDataDictionary(data.Item1, new RefitSettings()
            {
                JsonSerializerSettings = new JsonSerializerSettings()
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    Converters = { new StringEnumConverter(), new IsoDateTimeConverter() },
                    DateFormatHandling = DateFormatHandling.IsoDateFormat,
                    DateTimeZoneHandling = DateTimeZoneHandling.Utc
                }

            });

            var r1 = r.First();
            Assert.Equal(data.Item2.Key, r1.Key);
            Assert.Equal(data.Item2.Value, r1.Value);
        }



    }
}
