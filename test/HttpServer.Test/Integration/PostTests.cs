using FluentAssertions;
using HttpServer.Test.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace HttpServer.Test.Integration
{
    public class PostTests
    {
        private record TestDto(string Name);

        [Fact]
        public async Task POST_Reads_Json_And_Returns_It()
        {
            await using var server = new TestServer(async ctx =>  
            {
                TestDto? dto = await ctx.Request.ReadJsonAsync<TestDto>();
                await ctx.Response.WriteJsonAsync( new 
                {
                    received = dto?.Name
                });
            });

            var response = await server.Client.PostAsJsonAsync("/", new TestDto("Alice" ));
            response.EnsureSuccessStatusCode();

            var returnedContent = await response.Content.ReadAsStringAsync();
            returnedContent.Should().Contain("Alice");
        }
    }
}
