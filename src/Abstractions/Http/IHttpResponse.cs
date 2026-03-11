using Microsoft.Extensions.Primitives;

namespace Abstractions.Http
{
    public interface IHttpResponse
    {
        int StatusCode { get; set; }
        IDictionary<string, StringValues> Headers { get; }
        Stream Body { get; }
    }
}
