using Microsoft.Extensions.Primitives;

namespace Abstractions.Http
{
    public interface IHttpResponse
    {
        int StatusCode { get; set; }
        IHttpBaseDictionary Headers { get; }
        Stream Body { get; }

        Task WriteStringAsync(string text);
        Task WriteJsonAsync<T>(T value, CancellationToken cancellationToken = default);
        Task WriteBytesAsync(byte[] bytes, CancellationToken cancellationToken, string contentType = "application/octet-stream");
    }
}
