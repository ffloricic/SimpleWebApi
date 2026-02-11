namespace Abstractions.Hosting
{
    public interface IWebHost
    {
        Task StartAsync(CancellationToken cancellationToken);
    }

}
