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
    public class MultipartData<T> : IPart<T>
    {

        private MultipartData(T data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            var type = typeof(T);
            if (type.IsAssignableTo(typeof(Stream)) ||
                type.IsAssignableTo(typeof(FileInfo)) ||
                type.IsAssignableTo(typeof(byte[])) ||
                type.IsAssignableTo(typeof(IEnumerable<Stream>))||
                 type.IsAssignableTo(typeof(IEnumerable<FileInfo>)) ||
                  type.IsAssignableTo(typeof(IEnumerable<byte[]>))
               )
            {
                throw new ArgumentException($"Unexpected data type in a Multipart Data. {nameof(data)} can not be a type of or collection of String, Stream, FileInfo or Byte array");
            }

            Value = data;
        }
        public T Value { get; set; }
        object IPart.Value => Value;

        public static MultipartData<T> Create(T data)
        {
            return new MultipartData<T>(data);
        }
    }

    struct FormDataKeyItem
    {
        public FormDataKeyItem(string name)
        {
            Name = name;
            FileName = String.Empty;
        }
        public FormDataKeyItem(string name, string fileName)
        {
            Name = name;
            FileName = fileName;
        }

        public string Name { get;}
        public string FileName { get; }
    }
 
    class MultipartFormDataDictionary : Dictionary<FormDataKeyItem, object>
    {
        private readonly string _paramName;
        private static readonly Dictionary<Type, PropertyInfo[]> propertyCache
                = new Dictionary<Type, PropertyInfo[]>();

        public MultipartFormDataDictionary(string paramName, object source, RefitSettings settings)
        {
            this._paramName = paramName;
            if (source == null) return;

            var dictionary = source as IDictionary;

            if (dictionary != null)
            {
                var list = new List<KeyValuePair<FormDataKeyItem, object>>();
                foreach (var key in dictionary.Keys)
                {
                    list.Add(new KeyValuePair<FormDataKeyItem, object>(new FormDataKeyItem(key as string), dictionary[key]));
                }
                AddToFormData(settings, list);
                return;
            }

            var r = ConvertObjectToFlatPropertiesList(String.Empty, source);

            AddToFormData(settings, r);
        }

        private void AddToFormData(RefitSettings settings, List<KeyValuePair<FormDataKeyItem, object>> r)
        {
            foreach (var item in r)
            {
                var t = item.Value.GetType();

                if (CanConvertToString(t))
                {
                    var convertable = item.Value as IStringConvertable;
                    var val = convertable != null ? convertable.ConvertToString() : item.Value;
                    this.Add(item.Key, $"{val}");
                }
                else if (IsFile(t))
                {
                    this.Add(item.Key, item.Value);
                }
                else
                {
                    var o = JsonConvert.SerializeObject(item.Value, settings.JsonSerializerSettings);
                    o = o.Replace("\"", "");
                    this.Add(item.Key, o);
                }
            }
        }

        private List<KeyValuePair<FormDataKeyItem, object>> ConvertObjectToFlatPropertiesList(string paramName, object value)
        {
            var propertiesList = new List<KeyValuePair<FormDataKeyItem, object>>();
            FillFlatPropertiesListFromObject(value, paramName, String.Empty, propertiesList);

            return propertiesList;
        }

        private void FillFlatPropertiesListFromObject(object obj, string prefix, string alternativeName, List<KeyValuePair<FormDataKeyItem, object>> propertiesList)
        {
            if (obj != null)
            {
                Type type = obj.GetType();

                if (obj is IDictionary)
                {
                    var dict = obj as IDictionary;
                    int index = 0;
                    var pref = !String.IsNullOrWhiteSpace(prefix) ? prefix.ToCamelCase() : _paramName;

                    foreach (var key in dict.Keys)
                    {
                        string indexedKeyPropName = String.Format("{0}[{1}][key]", pref, index);
                        FillFlatPropertiesListFromObject(key, indexedKeyPropName, alternativeName, propertiesList);

                        string indexedValuePropName = String.Format("{0}[{1}][value]", pref, index);
                        FillFlatPropertiesListFromObject(dict[key], indexedValuePropName, alternativeName, propertiesList);

                        index++;
                    }
                }
                else if (obj is ICollection)
                {
                    var list = obj as ICollection;
                    int index = 0;
                    var pref = !String.IsNullOrWhiteSpace(prefix) ? prefix.ToCamelCase() : _paramName;

                    foreach (var indexedPropValue in list)
                    {
                        string indexedPropName = $"{pref}[{index}]";
                        FillFlatPropertiesListFromObject(indexedPropValue, indexedPropName, alternativeName, propertiesList);

                        index++;
                    }
                }
                else if (IsCustomNonEnumerableType(type))
                {
                    var props = GetPublicAccessibleProperties(type);
                  
                    foreach (var propertyInfo in props)
                    {
                        var jsonProperty = propertyInfo.GetCustomAttribute<JsonPropertyAttribute>();
                        var aliasAs = propertyInfo.GetCustomAttribute<AliasAsAttribute>();
                        var propertyName = aliasAs != null ?  aliasAs.Name : (jsonProperty != null ? jsonProperty.PropertyName : propertyInfo.Name.ToCamelCase());
                        string propName = String.IsNullOrWhiteSpace(prefix)
                            ? propertyName
                            : $"{prefix.ToCamelCase()}[{propertyName}]";

                        var attachmentName = propertyInfo.GetCustomAttribute<AttachmentNameAttribute>();
                        var fileName = attachmentName != null ? attachmentName.Name : alternativeName;
                        object propValue = propertyInfo.GetValue(obj);

                        FillFlatPropertiesListFromObject(propValue, propName, fileName, propertiesList);
                    }
                }
                else
                {
                    var pref = !String.IsNullOrWhiteSpace(prefix) ? prefix.ToCamelCase() : _paramName;
                    propertiesList.Add(new KeyValuePair<FormDataKeyItem, object>(new FormDataKeyItem(pref, alternativeName), obj));
                }
            }
        }

        private PropertyInfo[] GetReadableProperties(Type type)
        {
            return type.GetProperties()
                .Where(p => p.CanRead)
                .ToArray();
        }

        private IEnumerable<PropertyInfo> GetPublicAccessibleProperties( Type type)
        {
            lock (propertyCache)
            {
                if (!propertyCache.ContainsKey(type))
                {
                    propertyCache[type] = GetReadableProperties(type);
                }


                foreach (PropertyInfo propertyInfo in propertyCache[type])
                {
                    yield return propertyInfo;
                }

            }

        }

        private bool IsCustomNonEnumerableType(Type type)
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
                   && !IsFile(type)
                   && !type.IsAssignableTo(typeof(IStringConvertable))
                   && !type.GetInterfaces().Contains(typeof(IEnumerable));
        }

       
        private bool CanConvertToString(Type type)
        {
            if (type.IsAssignableTo(typeof(IStringConvertable)))
            {
                return true;
            }

            return type != typeof(DateTime)
                    && type != typeof(DateTimeOffset)
                    && !IsFile(type)
                    && (type.IsPrimitive() || type == typeof(string) || type == typeof(Guid) || type == typeof(TimeSpan));

        }

        public static bool IsFile(Type type)
        {
            return type == typeof(Stream)
#if !NETFX_CORE
                   || type == typeof(FileInfo);
#else 
            ;
#endif
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

    public interface IStringConvertable
    {
        string ConvertToString();
    }
}