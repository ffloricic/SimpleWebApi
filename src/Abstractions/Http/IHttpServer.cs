using Abstractions.Middleware;

namespace Abstractions.Http
{
    public interface IHttpServer
    {
        /// <summary>
        /// Starts accepting incoming connections.
        /// </summary>
        Task StartAsync(CancellationToken externalToken);
        ValueTask DisposeAsync(ShutdownMode mode = ShutdownMode.Graceful);
    }
}
