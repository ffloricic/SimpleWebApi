using Microsoft.Extensions.Primitives;

namespace Abstractions.Http
{
    public interface IHttpRequest
    {
        HttpRequestMethod Method { get; }
        string Path { get; } // Absolute path from URL
        string QueryString { get; } // Raw query string 
        IReadOnlyDictionary<string, StringValues> Query { get; } // Parsed query parameters
        IReadOnlyDictionary<string, StringValues> Headers { get; } // HTTP headers, case-insensitive
        Task<byte[]> ReadBodyAsync(); // Request body, transport owns lifetime
    }
}
