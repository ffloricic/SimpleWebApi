using Microsoft.Extensions.Primitives;
using System.Collections;
using System.Collections.Specialized;

namespace HttpServer
{
    public abstract class HttpBaseDictionary : IReadOnlyDictionary<string, StringValues>
    {
        protected readonly Dictionary<string, StringValues> _dictionary;

        protected HttpBaseDictionary()
        {
            _dictionary = new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase);
        }

        protected void Add(string key, StringValues values)
        {
            _dictionary.Add(
            key,
            values);
        }

        protected void Add(string key, string[]? values) =>
            _dictionary.Add(
                        key,
                        (values is null || values?.Length == 0)
                        ? StringValues.Empty : values);

        public StringValues this[string key] =>
            _dictionary.TryGetValue(key, out var value)
            ? value
            : StringValues.Empty;

        public IEnumerable<string> Keys => _dictionary.Keys;

        public IEnumerable<StringValues> Values => _dictionary.Values;

        public int Count => _dictionary.Count;

        public bool ContainsKey(string key) =>
            _dictionary.ContainsKey(key);

        public bool TryGetValue(string key, out StringValues value) =>
            _dictionary.TryGetValue(key, out value);

        public IEnumerator<KeyValuePair<string, StringValues>> GetEnumerator() =>
            _dictionary.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
