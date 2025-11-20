using System.Collections.Generic;

namespace Noodle.Api.Helpers
{
    public static class DictionaryExtensions
    {
        public static object? GetValueOrDefault(this IDictionary<string, object> dict, string key)
        {
            return dict != null && dict.TryGetValue(key, out var value) ? value : null;
        }
    }
}