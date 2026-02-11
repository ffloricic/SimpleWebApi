using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleWebApi.Abstractions.Http
{
    public interface IRequest
    {
        HttpMethod Method { get; }
        string Path { get; } // Absolute path from URL
        string QueryString { get; } // Raw query string 
        IReadOnlyDictionary<string, IReadOnlyList<string>> Query { get; } // Parsed query parameters
        IReadOnlyDictionary<string, IReadOnlyList<string>> Headers { get; } // HTTP headers, case-insensitive
        Stream Body { get; } // Request body, transport owns lifetime        
    }
}
