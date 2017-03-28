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

        public struct MyCustomValue : IStringConvertable
        {
            private readonly string _value;

            public MyCustomValue(string v)
            {
                _value = v;
            }
            public string ConvertToString()
            {
                return _value;
            }
        }

        private static DateTimeOffset d = new DateTimeOffset(2000, 12, 30, 12, 0, 0, TimeSpan.FromHours(8));
        private static Guid g = new Guid("E1A16EEB-CE85-4481-A71B-9F1ACBCB094E");
        private static TimeSpan t = TimeSpan.FromDays(1);
        private static FileInfo f = new FileInfo(@"C:\Temp\RawFile.txt");
        public static TheoryDataSet<Tuple<object, KeyValuePair<string, object>>> ObjectData => new TheoryDataSet<Tuple<object, KeyValuePair<string, object>>>()
        {
            new Tuple<object, KeyValuePair<string, object>>(
                new Dictionary<string, string>()
                {
                    {"Item1", "hello"}
                },
                new KeyValuePair<string, object>($"Item1", "hello")

            ),
            new Tuple<object, KeyValuePair<string, object>>(
            new DateData()
            {
                TestDate = d
            },
            new KeyValuePair<string, object>($"testDate", d.ToString("yyyy-MM-ddThh:mm:sszzzz"))
            ),
            new Tuple<object, KeyValuePair<string, object>>(
             new ParentObj()
            {
               Child = new ChildObject()
               {
                   ChildId = 1,
                   ChildName = "Hello"
               }
            },
              new KeyValuePair<string, object>($"child[childId]", $"{1}")
            ),
            new Tuple<object, KeyValuePair<string, object>>(
             new List<int> {1, 2, 3},
              new KeyValuePair<string, object>($"{paramName}[0]", $"{1}")
            ),
             new Tuple<object, KeyValuePair<string, object>>(
             new MyCustomValue("Dope"), 
              new KeyValuePair<string, object>($"{paramName}", $"Dope")
            ),
             new Tuple<object, KeyValuePair<string, object>>(
              t,
                new KeyValuePair<string, object>($"{paramName}",t.ToString())
            ),
             new Tuple<object, KeyValuePair<string, object>>(
              f, 
                new KeyValuePair<string, object>($"{paramName}",f)
            )


        };

      

        [Theory]
        [TestDataSet(typeof(MultiFormDataDictionaryTests), nameof(ObjectData))]
        public void ShouldFlattenPropertiesAsKeyValuePair(Tuple<object, KeyValuePair<string, object>> data)
        {
            var r = new MultiFormDataDictionary(paramName, data.Item1, new RefitSettings()
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
            Assert.Equal(data.Item2.Key, r1.Key.Name);
            Assert.Equal(data.Item2.Value, r1.Value);
        }



    }
}
