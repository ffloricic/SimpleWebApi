using Abstractions.Middleware;

namespace Abstractions.Routing
{
    public interface IEndpoint
    {
        HttpMethod Method { get; }
        string PathPattern { get; }
        RequestDelegate Handler { get; }
    }
}
