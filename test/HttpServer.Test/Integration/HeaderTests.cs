using FluentAssertions;
using HttpServer.Test.Server;

namespace HttpServer.Test.Integration
{

    public class HeaderTests
    {
        [Fact]
        public async Task Response_Include_Custom_Headers()
        {
            await using var testServer = new TestServer(async ctx => {
                ctx.Response.Headers["X-Custom-Header"] = "test-value";
                ctx.Response.Headers["X-Another-Header"] = "abc";

                await ctx.Response.WriteJsonAsync(new { ok = true });
            });

            var response = await testServer.Client.GetAsync("/");

            response.Headers.Contains("X-Custom-Header")
                .Should()
                .BeTrue();

            response.Headers.GetValues("X-Custom-Header")
                .Should()
                .ContainSingle()
                .Which.Should().Be("test-value");

            response.Headers.GetValues("X-Another-Header")
                .Should()
                .ContainSingle()
                .Which.Should().Be("abc");
        }

        [Fact]
        public async Task Response_Supports_Multiple_SetCookie_Headers()
        {
            await using var testServer = new TestServer(async ctx => {
                Console.WriteLine("HANDLER HIT");

                ctx.Response.Headers.Add("Set-Cookie", "session=abc; Path=/; HttpOnly");
                ctx.Response.Headers.Add("Set-Cookie", "theme=dark; Path=/");

                await ctx.Response.WriteJsonAsync (new { ok = true });
            });

            var response = await testServer.Client.GetAsync("/");

            response.Headers.TryGetValues("Set-Cookie", out var cookies).Should().BeTrue();

            cookies?.Count().Should().Be(2);
            cookies?.Should().Contain(c => c.Equals("session=abc; Path=/; HttpOnly"));
            cookies?.Should().Contain(c => c.Equals("theme=dark; Path=/"));
        }
    }
}
