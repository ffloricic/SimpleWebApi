using HttpServer.Test.Infrastructure;
using HttpServer.Test.Server;

namespace HttpServer.Test.Integration
{
    public class ClientDisconnectTests
    {
        [Fact]
        public async Task Server_Does_Not_Crash_Whan_Client_Disconnects_During_Response()
        {
            var requestStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

            await using var testServer = new TestServer(async ctx => {
                requestStarted.TrySetResult();

                // simulation of slow and large response
                var buffer = new byte[1024 * 1024]; // 1 MB

                for (int i = 0; i < 50; i++)
                {
                    await ctx.Response.Body.WriteAsync(buffer);
                    await Task.Delay(10); // slow down writing
                }
            });

            var cts = new CancellationTokenSource();
            var requestTask = testServer.Client.GetAsync("/", HttpCompletionOption.ResponseHeadersRead, cts.Token);

            await requestStarted.Task;

            // simulation of client disconection
            cts.Cancel();

            try
            {
                await requestTask;
            }
            catch
            {
                // expected (client side cancellation)
                TestHelper.Log("Client: Request Cancel Exception catched");
            }

            await Task.Delay(100);

            // verification that server still works after one connection cancellation
            var response = await testServer.Client.GetAsync("/");

            response.EnsureSuccessStatusCode();
        }
    }
}
