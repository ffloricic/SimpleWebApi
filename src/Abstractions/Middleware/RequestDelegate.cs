using Abstractions.Http;

namespace Abstractions.Middleware
{
    public delegate Task RequestHandler(IHttpContext context);
    
}
