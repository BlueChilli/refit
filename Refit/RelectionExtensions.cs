using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Refit
{
    public static class ReflectionExtensions
    {
        public static bool IsAssignableTo(this Type source, Type dest)
        {
            return dest.IsAssignableFrom(source);
        }

        public static bool IsInterface(this Type type)
        {
            return type.GetTypeInfo().IsInterface;
        }

        public static bool IsGenericType(this Type type)
        {
            return type.GetTypeInfo().IsGenericType;
        }

        public static bool IsPrimitive(this Type source)
        {
            return source.GetTypeInfo().IsPrimitive;
        }

        public static bool IsEnum(this Type source)
        {
            return source.GetTypeInfo().IsEnum;
        }
    }
}
