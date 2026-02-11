using Abstractions.Http;

namespace Abstractions.Middleware
{
    public delegate Task RequestDelegate(IHttpContext context);
}
