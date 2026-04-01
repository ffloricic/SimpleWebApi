using Abstractions.Http;
using FluentAssertions;
using HttpServer.Test.Server;
using System.Diagnostics;

namespace HttpServer.Test.Integration
{
    public class ShutdownTests
    {
        [Fact]
        public async Task Shutdown_Waits_For_InFlight_Requests_To_Complete()
        {
            var requestStarted = new TaskCompletionSource();
            var allowCompletion = new TaskCompletionSource();

            var testServer = new TestServer(async ctx => {
                requestStarted.SetResult();

                // simulate long running request
                await allowCompletion.Task;

                await ctx.Response.WriteJsonAsync(new { done = true });
            });

            var requestTask = testServer.Client.GetAsync("/");

            // ensure request is inside pipeline
            await requestStarted.Task;

            // trigger shutdown
            var disposeTask = testServer.DisposeHttpServerAsync(ShutdownMode.Graceful).AsTask();

            // Give shutdown a moment to potentially break things
            await Task.Delay(100);

            // allow request to finish
            allowCompletion.SetResult();

            var response = await requestTask;

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            content.Should().Contain("done");

            await disposeTask;

            await testServer.DisposeAsync();
        }

        [Fact]
         public async Task Shudown_Stops_Acceptiong_New_Requests()
        {
            var started = new TaskCompletionSource();

            var testServer = new TestServer(async ctx => {
                started.SetResult();
                await Task.Delay(500); // keep 1.st request busy for some time...
                await ctx.Response.WriteJsonAsync(new { ok = true });
            });

            var firstRequest = testServer.Client.GetAsync("/");
            await started.Task;

            // start shutdown
            var disposeTask = testServer.DisposeHttpServerAsync();

            await Task.Delay(100);

            //2.nd request should be not accepted
            /*Func<Task> act = async () => {
                await testServer.Client.GetAsync("/");
            };

            var ex = await act.Should().ThrowAsync<Exception>();

            ex.Which.Should().Match(e => e is HttpRequestException || e is TaskCanceledException);*/

            var result = await testServer.Client.GetAsync("/");
            result.StatusCode.Should().Be(System.Net.HttpStatusCode.ServiceUnavailable);

            await disposeTask;
            await testServer.DisposeAsync();
        }

        [Fact]
        public async Task Shudown_Cancels_InFlight_Requests()
        {
            var cancellationObserved = new TaskCompletionSource();

            var testServer = new TestServer(async ctx =>
            {
                try
                {
                    while (!ctx.RequestCancellationToken.IsCancellationRequested)
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(3));
                    }

                    cancellationObserved.SetResult();
                }
                catch (OperationCanceledException)
                {
                    cancellationObserved.SetResult();
                    Debug.WriteLine("Shudown_Cancels_InFlight_Requests: cancellation requested");
                }
            });

            var requestTask = testServer.Client.GetAsync("/");

            // ensure that request had started
            await Task.Delay(100);

            var disposeTask = testServer.DisposeHttpServerAsync(ShutdownMode.Immediate).AsTask();

            await cancellationObserved.Task;

            await disposeTask;

            await testServer.DisposeAsync();
        }
    }
}
