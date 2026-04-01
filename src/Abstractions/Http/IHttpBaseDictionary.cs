using Microsoft.Extensions.Primitives;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abstractions.Http
{
    public interface IHttpBaseDictionary : IReadOnlyDictionary<string, StringValues>
    {
        new StringValues this[string key] { get; set; }

        void Add(string key, StringValues newValues);

        void Add(string key, string[]? values);

        void Add(string key, string value);

        bool Remove(string key);
    }
}
