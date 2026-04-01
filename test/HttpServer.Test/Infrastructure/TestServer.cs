using Abstractions.Http;
using Abstractions.Middleware;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace HttpServer.Test.Server
{
    public sealed class TestServer: IAsyncDisposable
    {
        private readonly HttpServer _server;
        private readonly CancellationTokenSource _cTs = new();
        private readonly Task _serverTask;

        public HttpClient Client { get; }
        public string BaseUrl { get; }

        public TestServer(RequestHandler handler, ILoggerFactory? factory = null)
        {
            var loggerFactory = factory ?? LoggerFactory.Create(builder => {
                builder
                    .SetMinimumLevel(LogLevel.Debug)
                    .AddConsole();
            });

            var logger = loggerFactory.CreateLogger<HttpServer>();

            var port = GetFreePort();
            BaseUrl = $"http://localhost:{port}/";

            _server = new HttpServer(BaseUrl, handler, logger);
            _serverTask = _server.StartAsync(_cTs.Token);

            Client = new HttpClient()
            {
                BaseAddress = new Uri(BaseUrl)
            };
        }

        public async ValueTask DisposeHttpServerAsync(ShutdownMode mode = ShutdownMode.Graceful)
        {
            try
            {
                await _server.DisposeAsync(mode);
            }
            catch
            {
                // swallow expected cancellation exceptions
            }
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                if (_server.State != ServerState.Stopped)
                {
                    await DisposeHttpServerAsync(ShutdownMode.Immediate);
                }

                Client.Dispose();
                _cTs.Dispose();
            }
            catch
            {
                // swallow expected cancellation exceptions
            }
        }

        private static int GetFreePort()
        {
            var listener = new System.Net.Sockets.TcpListener(
                System.Net.IPAddress.Loopback, 0);

            listener.Start();
            var port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();

            return port;
        }
    }
}
