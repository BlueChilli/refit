using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Refit
{
    public static class UriExtensions
    {
        public static string TrimEndSlash(this Uri uri)
        {
            return uri.AbsoluteUri.EndsWith("/") ? uri.AbsoluteUri.TrimEnd('/') : uri.AbsoluteUri;
        }

        public static string TrimStartSlash(this Uri uri)
        {
            return uri.AbsoluteUri.StartsWith("/") ? uri.AbsoluteUri.TrimStart('/') : uri.AbsoluteUri;
        }

        public static string TrimEndSlash(this string uri)
        {
            return uri.EndsWith("/") ? uri.TrimEnd('/') : uri;
        }

        public static string TrimStartSlash(this string uri)
        {
            return uri.StartsWith("/") ? uri.TrimStart('/') : uri;
        }
    }
}
