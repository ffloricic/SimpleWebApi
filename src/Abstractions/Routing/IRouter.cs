using Abstractions.Http;

namespace Abstractions.Routing
{
    public interface IRouter
    {
        Task<IEndpoint?> MatchAsync(IHttpContext context);
    }
}
