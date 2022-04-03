using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace FluentState.Extensions
{
    public static class NewtonsoftExtensions
    {
        public static IEnumerable<T>? NonNullableValues<T>(this JToken obj, string key)
        {
            return obj[key]?.Values<T>().Where(a => a != null).Cast<T>().ToList() ?? null;
        }

        public static IEnumerable<T>? NonNullableValues<T>(this JToken obj)
        {
            return obj.Values<T>().Where(a => a != null).Cast<T>().ToList() ?? null;
        }
    }
}
