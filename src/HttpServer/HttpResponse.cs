using Abstractions.Http;
using Microsoft.Extensions.Primitives;
using System.Net;
using System.Text;
using System.Text.Json;

namespace HttpServer
{
    public class HttpResponse : IHttpResponse
    {
        private readonly HttpListenerResponse _nativeResponse;
        private readonly MemoryStream _buffer;
        public int StatusCode { get; set; } = 200;

        public IHttpBaseDictionary Headers { get; }

        public Stream Body  => _buffer;

        public HttpResponse(HttpListenerResponse response)
        {
            ArgumentNullException.ThrowIfNull(response);

            _nativeResponse = response;
            _buffer = new MemoryStream();
            Headers = new HttpHeadersDictionary();
        }

        public void AddCookie(string cookie)
        {
            Headers["Set-Cookie"] = StringValues.Concat(Headers["Set-Cookie"], cookie);
        }

        public async Task WriteStringAsync(string text)
        {
            if (!Headers.ContainsKey("Content-Type"))
            {
                Headers["Content-Type"] = "text/plain; charset=utf-8";
            }

            var bytes = Encoding.UTF8.GetBytes(text);
            await _buffer.WriteAsync(bytes);
        }

        public async Task WriteJsonAsync<T>(T value, CancellationToken cancellationToken = default)
        {
            if (!Headers.ContainsKey("Content-Type"))
            {
                Headers["Content-Type"] = "application/json; charset=utf-8";
            }

            await JsonSerializer.SerializeAsync(_buffer, value, cancellationToken: cancellationToken);
        }

        public async Task WriteBytesAsync(
            byte[] bytes,            
            CancellationToken cancellationToken,
            string contentType = "application/octet-stream")
        {
            Headers["Content-Type"] = contentType;
            await _buffer.WriteAsync(bytes, cancellationToken: cancellationToken);
        }
    }
}
