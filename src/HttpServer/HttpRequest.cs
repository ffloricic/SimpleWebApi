using Abstractions.Http;
using Microsoft.Extensions.Primitives;
using System.Buffers;
using System.Net;
using System.Text;
using System.Text.Json;

namespace HttpServer
{
    public class HttpRequest : IHttpRequest
    {
        // TODO: put this constant into configuration file
        const int MaxBodySize = 10 * 1024 * 1024;

        private readonly HttpListenerRequest _request;
        private readonly CancellationToken _cancellationToken;
        private readonly Stream _body;
        private byte[]? _cachedBody;

        public HttpRequestMethod Method { get; }

        public string Path { get; }

        public string? QueryString { get; }

        public IReadOnlyDictionary<string, StringValues>? Query { get; }

        public IReadOnlyDictionary<string, StringValues>? Headers { get; }

        internal Stream? Body => _body;

        public HttpRequest(HttpListenerRequest request, CancellationToken cancellationToken)
        {
            _request = request;
            _cancellationToken = cancellationToken;

            Method = Enum.TryParse<HttpRequestMethod>(request.HttpMethod, out var value)
                        && Enum.IsDefined(typeof(HttpRequestMethod), value)
                            ? value
                            : throw new InvalidOperationException();

            Path = request.Url!.AbsolutePath;
            QueryString = request.Url!.Query;

            Query = new HttpQueryCollection(request.Url!.Query);

            Headers = new HttpHeadersDictionary(request.Headers);

            _body = request.InputStream;
            _cancellationToken = cancellationToken;
        }

        public async Task<byte[]> ReadBodyAsync()
        {
            if (_cachedBody != null)
            {
                return _cachedBody;
            }

            if (!_request.HasEntityBody)
            {
                return [];
            }

            if (_request.ContentLength64 > 0 &&
                _request.ContentLength64 > MaxBodySize)
            {
                throw new InvalidOperationException(
                    $"Request body too large. Limit: {MaxBodySize} bytes");
            }

            var pool = ArrayPool<byte>.Shared;
            byte[] buffer = pool.Rent(8192);

            try
            {
                using var mS = new MemoryStream(Math.Max(0, (int)_request.ContentLength64));
                
                while (true)
                {
                    int read = await _body.ReadAsync(
                                                buffer,
                                                0,
                                                buffer.Length,
                                                _cancellationToken);
                    if (read <= 0)
                        break;

                    await mS.WriteAsync(
                        buffer.AsMemory(0, read),
                        _cancellationToken);
                }

                _cachedBody = mS.ToArray();
                return _cachedBody;
            }
            finally
            {
                pool.Return(buffer);
            }
        }

        public async Task<string> ReadBodyAsStringAsync()
        {
            var byteArr = await ReadBodyAsync();
            return Encoding.UTF8.GetString(byteArr);
        }

        public async Task<T?> ReadJsonAsync<T>()
        {
            var byteArr = await ReadBodyAsync();

            return JsonSerializer.Deserialize<T>(
                new ReadOnlySpan<byte>(byteArr),
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                });
        }
    }
}
