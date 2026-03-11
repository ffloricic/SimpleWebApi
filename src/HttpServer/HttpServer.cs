using Abstractions.Http;
using Abstractions.Middleware;
using System.Collections.Concurrent;
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

        private HttpListener _listener;
        private CancellationTokenSource _cts;
        private RequestHandler _requestHandler;
        private readonly ConcurrentDictionary<Task, byte> _activeRequests = new();

        public HttpServer(
            string prefix,
            RequestHandler requestHandler)
        {
            // TODO: prefix Url should be stored in application.settings file
            // TODO: check first if prefix is in corect format
            // TODO: if not. Log the situation and raise exception
            _listener = new HttpListener();
            _listener.Prefixes.Add(prefix);

            _requestHandler = requestHandler;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            _listener.Start();
            // TODO: log listener start
            RegisterListenerStopProcedure();

            try
            {
                while (!_cts.IsCancellationRequested)
                {
                    var httpContext = await _listener.GetContextAsync();
                    var task = ProcessRequestAsync(httpContext, _cts.Token);
                    AddTaskToActiveRequests(task);

                    _ = task.ContinueWith(t => {
                        RemoveTaskFromActiveRequests(t);

                    }, TaskContinuationOptions.ExecuteSynchronously);
                }
            }
            catch (HttpListenerException)
            {
                // TODO: log when listener Stop procedure is called
            }
            finally
            {
                await WaitActiveRequestsToStop();
            }
        }

        private async Task ProcessRequestAsync(
            HttpListenerContext httpListenerContext,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(httpListenerContext);

            var context = new HttpContext(httpListenerContext, cancellationToken);

            try
            {
                await _requestHandler(context);
                await WriteResponseAsync(httpListenerContext, context.Response, _cts.Token);
            }
            catch (OperationCanceledException) when (_cts.Token.IsCancellationRequested)
            {
                // expected during shutdown
                // TODO: -> log 
            }
            catch (IOException ex)
            {
                // client disconnected
                // TODO: -> log 
            }
            catch (HttpListenerException ex)
            {
                // aborted request
                // TODO: -> log 
            }
            catch (Exception ex)
            {
                // TODO: log, possibly write 500
            }
        }

        private void RegisterListenerStopProcedure()
        {
            try
            {
                _cts.Token.Register(() => {
                    _listener.Stop();
                });
            }
            catch
            {
                // TODO: -> log listener stop
            }
        }

        private void AddTaskToActiveRequests(Task newTask)
        {
            _activeRequests.TryAdd(newTask, 0);
        }

        private void RemoveTaskFromActiveRequests(Task task)
        {
            _activeRequests.TryRemove(task, out _);
        }

        private async Task WaitActiveRequestsToStop()
        {
            if (_activeRequests.IsEmpty)
            {
                return;
            }

            try
            {
                await Task.WhenAny(
                    Task.WhenAll(_activeRequests.Keys),
                    Task.Delay(TimeSpan.FromSeconds(30/* TODO: -> put that constant in config file*/))
                    );
            }
            catch
            {
                // Ignore — requests may fail during shutdown
            }
        }

        public async Task StopAsync()
        {
            _cts.Cancel();

            try
            {
                _listener.Stop();
            }
            catch
            { }

            await WaitActiveRequestsToStop();
            _listener.Close();
        }

        public async Task WriteResponseAsync(
            HttpListenerContext listenerContext,
            IHttpResponse response,
            CancellationToken cancellationToken)
        {
            var nativeResponse = listenerContext.Response;

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

            // set Content-length automatically
            nativeResponse.ContentLength64 = response.Body.Length;

            // reset position
            response.Body.Position = 0;

            // copy buffer to network stream
            await response.Body.CopyToAsync(nativeResponse.OutputStream, cancellationToken);

            // flush and close
            await nativeResponse.OutputStream.FlushAsync(cancellationToken);
            nativeResponse.OutputStream.Close();
            nativeResponse.Close();
        }
    }
}
