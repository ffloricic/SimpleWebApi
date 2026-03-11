using Abstractions.Http;

namespace Abstractions.Middleware
{
    public interface IMiddleware
    {
        Task InvokeAsync(IHttpContext context, RequestHandler next);
    }
}
