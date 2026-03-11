using Abstractions.Http;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using System.Threading;

namespace HttpServer
{
    public class HttpContext: IHttpContext
    {
        private readonly CancellationToken _cancellationToken;
        public IHttpRequest Request { get; }
        public IHttpResponse Response { get; }

        public CancellationToken CancellationToken  => _cancellationToken;
        public HttpContext(
            HttpListenerContext listenerContext,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(listenerContext);

            _cancellationToken = cancellationToken;

            Request = new HttpRequest(listenerContext.Request, cancellationToken);
            Response = new HttpResponse(listenerContext.Response);
        }
    }
}
