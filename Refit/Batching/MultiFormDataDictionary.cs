using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace Refit
{
    public interface IPart
    {
        object Value { get; }
    }
    public interface IPart<T> : IPart
    {
        new T Value { get; set; }
    }
    public class MultiPartData<T> : IPart<T>
    {

        private MultiPartData(T data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            var type = typeof(T);
            if (type.IsAssignableTo(typeof(Stream)) ||
                type.IsAssignableTo(typeof(FileInfo)) ||
                type.IsAssignableTo(typeof(byte[])) ||
                 type.IsAssignableTo(typeof(string)) ||
                type.IsAssignableTo(typeof(IEnumerable<Stream>))||
                 type.IsAssignableTo(typeof(IEnumerable<FileInfo>)) ||
                  type.IsAssignableTo(typeof(IEnumerable<byte[]>)) ||
                type.IsAssignableTo(typeof(IEnumerable<string>)))
            {
                throw new ArgumentException($"Unexpected data type in a Multipart Data. {nameof(data)} can not be a type of or collection of String, Stream, FileInfo or Byte array");
            }

            Value = data;
        }
        public T Value { get; set; }
        object IPart.Value => Value;

        public static MultiPartData<T> Create(T data)
        {
            return new MultiPartData<T>(data);
        }
    }

    internal static class TypeExtensions
    {
        private static readonly Dictionary<Type, PropertyInfo[]> propertyCache
                = new Dictionary<Type, PropertyInfo[]>();

        internal static PropertyInfo[] GetReadableProperties(this Type type)
        {
            return type.GetProperties()
                .Where(p => p.CanRead)
                .ToArray();
        }

        internal static IEnumerable<PropertyInfo> GetPublicAccessibleProperties(this Type type)
        {
            lock (propertyCache)
            {
                if (!propertyCache.ContainsKey(type))
                {
                    propertyCache[type] = type.GetReadableProperties();
                }


                foreach (PropertyInfo propertyInfo in propertyCache[type])
                {
                    yield return propertyInfo;
                }

            }

        }

        internal static bool IsCustomNonEnumerableType(this Type type)
        {
            var nullType = Nullable.GetUnderlyingType(type);
            if (nullType != null)
            {
                type = nullType;
            }
            if (type.IsGenericType())
            {
                type = type.GetGenericTypeDefinition();
            }
            return type != typeof(object)
                   && type != typeof(Guid)
                   && type != typeof(DateTime)
                   && type != typeof(DateTimeOffset)
                   && type != typeof(TimeSpan)
                   && type != typeof(string)
                   && !type.IsPrimitive()
                   && !type.GetInterfaces().Contains(typeof(IEnumerable));
        }
    }
    class MultiFormDataDictionary : Dictionary<string, string>
    {
        public MultiFormDataDictionary(object source, RefitSettings settings)
        {
            if (source == null) return;

            var dictionary = source as IDictionary;

            if (dictionary != null)
            {
                foreach (var key in dictionary.Keys)
                {
                    Add(key.ToString(), string.Format("{0}", dictionary[key]));
                }

                return;
            }

            var r = ConvertObjectToFlatPropertiesList(String.Empty, source);

            foreach (var item in r)
            {
                var t = item.Value.GetType();

                if ((t.IsPrimitive() || t == typeof(string)) && t != typeof(DateTime) && t != typeof(DateTimeOffset))
                {
                    this.Add(item.Key, $"{item.Value}");
                }
                else
                {
                    var o = JsonConvert.SerializeObject(item.Value, settings.JsonSerializerSettings);
                     o = o.Replace("\"", "");
                    this.Add(item.Key, o);
                }
            }
        }

       
        private List<KeyValuePair<string, object>> ConvertObjectToFlatPropertiesList(string paramName, object value)
        {
            var propertiesList = new List<KeyValuePair<string, object>>();
            FillFlatPropertiesListFromObject(value, paramName, propertiesList);

            return propertiesList;
        }

        private void FillFlatPropertiesListFromObject(object obj, string prefix, List<KeyValuePair<string, object>> propertiesList)
        {
            if (obj != null)
            {
                Type type = obj.GetType();

                if (obj is IDictionary)
                {
                    var dict = obj as IDictionary;
                    int index = 0;
                    foreach (var key in dict.Keys)
                    {
                        string indexedKeyPropName = String.Format("{0}[{1}][key]", prefix.ToCamelCase(), index);
                        FillFlatPropertiesListFromObject(key, indexedKeyPropName, propertiesList);

                        string indexedValuePropName = String.Format("{0}[{1}][value]", prefix.ToCamelCase(), index);
                        FillFlatPropertiesListFromObject(dict[key], indexedValuePropName, propertiesList);

                        index++;
                    }
                }
                else if (obj is ICollection)
                {
                    var list = obj as ICollection;
                    int index = 0;
                    foreach (var indexedPropValue in list)
                    {
                        string indexedPropName = String.Format("{0}[{1}]", prefix.ToCamelCase(), index);
                        FillFlatPropertiesListFromObject(indexedPropValue, indexedPropName, propertiesList);

                        index++;
                    }
                }
                else if (type.IsCustomNonEnumerableType())
                {
                    foreach (var propertyInfo in type.GetPublicAccessibleProperties())
                    {
                        string propName = String.IsNullOrWhiteSpace(prefix)
                            ? propertyInfo.Name
                            : String.Format("{0}[{1}]", prefix.ToCamelCase(), propertyInfo.Name.ToCamelCase());
                        object propValue = propertyInfo.GetValue(obj);

                        FillFlatPropertiesListFromObject(propValue, propName, propertiesList);
                    }
                }
                else
                {
                    propertiesList.Add(new KeyValuePair<string, object>(prefix.ToCamelCase(), obj));
                }
            }
        }
    }

    static class StringExtensions
    {
        public static string ToCamelCase(this string source)
        {
            if (!String.IsNullOrWhiteSpace(source))
            {
                return string.Format("{0}{1}", source[0].ToString().ToLower(), source.Substring(1, source.Length - 1));
            }
            return source;
        }
    }
}