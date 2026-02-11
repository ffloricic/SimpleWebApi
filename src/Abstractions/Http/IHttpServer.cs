namespace Abstractions.Http
{
    public interface IHttpServer
    {
        /// <summary>
        /// Starts accepting incoming connections.
        /// This method MUST NOT wait for request execution to complete.
        /// </summary>
        Task StartAsync(
            Func<IHttpContext, Task> requestHandler,
            CancellationToken cancellationToken);
    }
}
