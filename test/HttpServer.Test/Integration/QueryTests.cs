using FluentAssertions;
using HttpServer.Test.Server;

namespace HttpServer.Test.Integration
{
    public class QueryTests
    {
        [Fact]
        public async Task Query_Parses_Simple_Key_Value()
        {
            await using var testServer = new TestServer(async ctx => {
                var value = ctx.Request.Query["name"];

                await ctx.Response.WriteJsonAsync(new { name = value });
            });

            var response = await testServer.Client.GetAsync("/?name=Alice");

            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("Alice");
        }

        [Fact]
        public async Task Query_Decodes_Url_Encoded_Value()
        {
            await using var testServer = new TestServer(async ctx => {
                var value = ctx.Request.Query["q"];

                await ctx.Response.WriteJsonAsync(new { value });
            });

            var response = await testServer.Client.GetAsync("/?q=hello%20world");
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("hello world");
        }

        [Fact]
        public async Task Quey_Handles_Empty_Value()
        {
            await using var testServer = new TestServer(async ctx => {
                var value = ctx.Request.Query["empty"];
                await ctx.Response.WriteJsonAsync(new { value });
            });

            var response = await testServer.Client.GetAsync("/?empty=");
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("\"\"");
        }

        [Fact]
        public async Task Query_Handles_Key_Without_Value()
        {
            await using var testServer = new TestServer(async ctx => {
                var value = ctx.Request.Query["flag"];
                await ctx.Response.WriteJsonAsync(new {value});
            });

            var response = await testServer.Client.GetAsync("/?flag");
            var content = await response.Content.ReadAsStringAsync();

            content.Should().Contain("[]"); // i.e. empty stringValues value -> []
        }

        [Fact]
        public async Task Query_Suppoert_Duplicate_Keys()
        {
            await using var testServer = new TestServer(async ctx => {
                var values = ctx.Request.Query["id"];
                await ctx.Response.WriteJsonAsync(new { values });
            });

            var result = await testServer.Client.GetAsync("/?id=1&id=2");
            var content = await result.Content.ReadAsStringAsync();

            content.Should().Contain("{\"values\":[\"1\",\"2\"]}");

        }

        [Fact]
        public async Task Query_Parses_Multiple_Parameters()
        {
            await using var testServer = new TestServer(async ctx => {
                var a = ctx.Request.Query["a"];
                var b = ctx.Request.Query["b"];

                await ctx.Response.WriteJsonAsync(new { a, b });
            });

            var response = await testServer.Client.GetAsync("/?a=1&b=2");
            var content = await response.Content.ReadAsStringAsync();

            content.Should().Contain("{\"a\":[\"1\"],\"b\":[\"2\"]}");
        }

        [Fact]
        public async Task Query_Is_Case_Insensitive()
        {
            await using var testServer = new TestServer(async ctx => {
                var value = ctx.Request.Query["NAME"];
                await ctx.Response.WriteJsonAsync(new { value }); 
            });

            var result = await testServer.Client.GetAsync("/?name=Alice");
            var content = await result.Content.ReadAsStringAsync();

            content.Should().Contain("Alice");
        }

        [Fact]
        public async Task Query_Handels_Special_Characters()
        {
            await using var testServer = new TestServer(async ctx =>
            {
                var value = ctx.Request.Query["sym"];

                await ctx.Response.WriteJsonAsync(new { value });
            });

            var result = await testServer.Client.GetAsync("/?sym=%40%23%24");
            var content = await result.Content.ReadAsStringAsync();

            content.Should().Contain("{\"value\":[\"@#$\"]}");            
        }
    }
}
