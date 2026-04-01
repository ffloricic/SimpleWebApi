using Abstractions.Http;
using Microsoft.Extensions.Primitives;
using System.Collections;
using System.Collections.Specialized;
using System.Net;

namespace HttpServer
{
    public abstract class HttpBaseDictionary : IHttpBaseDictionary
    {
        protected readonly Dictionary<string, StringValues> _dictionary;

        protected HttpBaseDictionary()
        {
            _dictionary = new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase);
        }

        public void Add(string key, StringValues newValues)
        {
            if (_dictionary.TryGetValue(key, out _))
            {
                _dictionary[key] = StringValues.Concat(_dictionary[key], newValues);
            }
            else
            {
                _dictionary[key] = newValues;
            }
        }

        public void Add(string key, string[]? values) =>
            _dictionary.Add(
                        key,
                        new StringValues((values is null || values?.Length == 0) ? StringValues.Empty : values));

        public void Add(string key, string value)
        {
            Add(key, new StringValues(value));
        }

        public StringValues this[string key]
        {
            get
            {
                return _dictionary.TryGetValue(key, out var value)
                                    ? value
                                    : StringValues.Empty;
            }
            set
            {
                Add(key, value);
            }
        }

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

        public bool Remove(string key)
        {
            return _dictionary.Remove(key);
        }
    }
}
