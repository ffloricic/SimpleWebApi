using FluentAssertions;
using HttpServer.Test.Server;
using System.Diagnostics;
using System.Net;

namespace HttpServer.Test.Integration
{

    public class GetTests
    {
        [Fact]
        public async Task Get_Returns_200_and_Body()
        {
            var server = new TestServer(async ctx => 
            {
                ctx.Response.StatusCode = 200;
                await ctx.Response.WriteJsonAsync(new { message = "Hello World" });
            });

            var response = await server.Client.GetAsync("/");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("Hello World");

            await server.DisposeAsync();
        }
    }
}
