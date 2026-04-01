using Abstractions.Http;
using Abstractions.Middleware;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;

namespace HttpServer
{
#warning before final version don't forget to finish TODO list
    public class HttpServer : IHttpServer
    {
        // TODO: -> put into configuration file 
        static readonly HashSet<string> RestrictedHeaders = [
            "Transfer-Encoding",
            "Connection",
            "Date",
            "Content-Length"
        ];

        private readonly HttpListener _listener;
        private CancellationTokenSource _shutDownCts;
        private readonly RequestHandler _requestHandler;
        private readonly ConcurrentDictionary<Task, /*byte*/CancellationTokenSource> _activeRequests = new();
        private readonly ILogger<HttpServer> _logger;
        private volatile ServerState _state = ServerState.Created;
        private ShutdownMode _shutdownMode = ShutdownMode.Graceful;
        /* TODO: -> put that constant in config file*/
        private readonly TimeSpan _shutdownTimeout = TimeSpan.FromSeconds(30);
        private readonly TimeSpan _requestRecallTimeout = TimeSpan.FromSeconds(30);
        public ServerState State => _state;

        public HttpServer(
            string prefix,
            RequestHandler requestHandler,
            ILogger<HttpServer>? logger)
        {
            // TODO: prefix Url should be stored in application.settings file
            // TODO: check first if prefix is in corect format
            // TODO: if not. Log the situation and raise exception
            _listener = new HttpListener();
            _listener.Prefixes.Add(prefix);
            _requestHandler = requestHandler;
            _logger = logger ?? NullLogger<HttpServer>.Instance;
        }

        public async Task StartAsync(CancellationToken externalToken)
        {
            if (_state != ServerState.Created)
                throw new InvalidOperationException("Server already started...");

            _shutDownCts = CancellationTokenSource.CreateLinkedTokenSource(externalToken);
            //_shutDownToken = _cts.Token;

            _listener.Start();
            _state = ServerState.Running;
            _logger.LogInformation("Server started on {Url}", _listener.Prefixes.First());

            await AcceptLoopAsync();
        }

        private async Task AcceptLoopAsync()
        {
            HttpListenerContext? httpContext = null;

            while (true)
            {
                try
                {
                    httpContext = await _listener.GetContextAsync();

                    _logger.LogDebug("Request received: {Method} {Path}",
                        httpContext.Request.HttpMethod,
                        httpContext.Request.Url?.AbsolutePath);
                }
                catch (HttpListenerException ex) when (_state != ServerState.Running)
                {
                    _logger.LogInformation(ex, "HttpListenerException: {Message}", ex.Message);
                    break;
                }
                catch (ObjectDisposedException ex) when (_state != ServerState.Running)
                {
                    _logger.LogInformation(ex, "Exception in HttpServer process loop: {Message}", ex.Message);
                    break;
                }

                if (httpContext == null)
                    continue;

                if (_state != ServerState.Running)
                {
                    RejectRequest(httpContext);
                }

                _ = ProcessRequestTrackedAsync(httpContext);
            }
        }

        private void RejectRequest(HttpListenerContext context)
        {
            try
            {
                context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                context.Response.Close();
            }
            catch (Exception ex)
            {
                _logger.LogDebug($"Failed to reject request: {ex.Message}");
            }
        }

        private async Task ProcessRequestTrackedAsync(HttpListenerContext context)
        {
            // TODO: -> put the constant below into configuration file 
            using var requestOwnCts = new CancellationTokenSource(_requestRecallTimeout);

            var task = ProcessRequestAsync(context, requestOwnCts);
            AddTaskToActiveRequests(task, requestOwnCts);

            try
            {
                await task;
            }
            catch (Exception ex)
            {
                RemoveTaskFromActiveRequests(task);
            }
        }

        private async Task ProcessRequestAsync(
            HttpListenerContext httpListenerContext,
            CancellationTokenSource requestCts)
        {
            ArgumentNullException.ThrowIfNull(httpListenerContext);

            try
            {
                var context = new HttpContext(httpListenerContext, requestCts.Token);

                using (_logger.BeginScope(new Dictionary<string, object>
                {
                    ["TraceId"] = Guid.NewGuid(),
                    ["Path"] = httpListenerContext.Request.Url?.AbsolutePath ?? ""
                }))
                {
                    await _requestHandler(context);
                }

                await WriteResponseAsync(httpListenerContext.Response, context.Response);
                _logger.LogInformation("Response sent: {StatusCode}", httpListenerContext.Response.StatusCode);
            }
            catch (IOException ex)
            {
                requestCts.Cancel();
                _logger.LogDebug("Client disconnected (IO).");
            }
            catch (HttpListenerException ex)
            {
                requestCts.Cancel();
                _logger.LogDebug("Client disconnected (listener).");
            }
            catch (OperationCanceledException)
            {
                _logger.LogDebug("Request cancelled.");
            }
            catch (Exception ex)
            {
                // TODO: possibly write 500
                _logger.LogInformation(ex, "Unhalted exception during request");
            }
        }

        private void AddTaskToActiveRequests(Task newTask, CancellationTokenSource cts)
        {
            _activeRequests.TryAdd(newTask, cts);
        }

        private void RemoveTaskFromActiveRequests(Task task)
        {
            _activeRequests.TryRemove(task, out _);
        }

        private async Task WaitActiveRequestsToStop()
        {
            _logger.LogInformation("Waiting for {Count} active request", _activeRequests.Count);

            if (_activeRequests.IsEmpty)
            {
                return;
            }

            _logger.LogDebug("Waiting for {Count} active request", _activeRequests.Count);

            await Task.WhenAll([.. _activeRequests.Keys]);

            _logger.LogDebug("Number of active request: {Count}", _activeRequests.Count);
        }

        public async Task WriteResponseAsync(
            HttpListenerResponse nativeResponse,
            IHttpResponse response)
        {
            // apply status code
            nativeResponse.StatusCode = response.StatusCode;

            // apply headers
            foreach (var header in response.Headers)
            {
                var key = header.Key;

                // omit headers automatically set by httpLister and HTTP protocol
                if (RestrictedHeaders.Contains(key))
                {
                    continue;
                }

                if (key.Equals("Content-type", StringComparison.OrdinalIgnoreCase))
                {
                    nativeResponse.ContentType = header.Value.ToString();
                    continue;
                }

                if (key.Equals("Set-Cookie", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (var value in header.Value)
                    {
                        nativeResponse.Headers.Add(header.Key, value);
                    }
                }
                else
                {
                    nativeResponse.Headers.Add(key, header.Value.ToString());
                }
            }

            if (response.Body != null)
            {
                // set Content-length automatically
                nativeResponse.ContentLength64 = response.Body.Length;

                // reset position
                response.Body.Position = 0;

                // copy buffer to network stream
                try
                {
                    await response.Body.CopyToAsync(nativeResponse.OutputStream);
                    await nativeResponse.OutputStream.FlushAsync();
                }
                catch (ObjectDisposedException)
                {
                    _logger.LogDebug("Connection closed while writing response.");
                }
                catch (HttpListenerException)
                {
                    _logger.LogDebug("Client disconected during response write.");
                }
                catch (IOException)
                {
                    _logger.LogDebug("I/O aborted (likely client disconect).");
                }

            }

            nativeResponse.OutputStream.Close();
            nativeResponse.Close();
        }

        public async ValueTask DisposeAsync(ShutdownMode mode = ShutdownMode.Graceful)
        {
            if (_state == ServerState.Stopped)
                return;

            _shutdownMode = mode;
            _state = ServerState.Stopping;

            _logger.LogInformation("Server shutting down... Mode: {Mode}", mode);

            if (mode == ShutdownMode.Immediate)
            {
                foreach(CancellationTokenSource cts in _activeRequests.Values)
                {
                    cts.Cancel();
                }
            }

            var waitActiveTasks = WaitActiveRequestsToStop();

            // wait with timeout
            var completed = await Task.WhenAny(waitActiveTasks, Task.Delay(_shutdownTimeout));

            // this is mostly for the case of gracefull shutdown but wouldn't bother also in the case of immediate
            if (completed != waitActiveTasks)
            {
                _logger.LogWarning("Shutdown timeout reached. Cancelling remaining requests.");

                foreach (CancellationTokenSource cts in _activeRequests.Values)
                {
                    cts.Cancel();
                }

                // now must complete
                await waitActiveTasks;
            }

            try
            {
                _listener.Stop();
                _listener.Close();
            }
            catch { }

            _state = ServerState.Stopped;

            try
            {
                _listener.Close();
            }
            catch { }

            _logger.LogInformation("Server stopped.");

            _shutDownCts.Dispose();
        }
    }
}
