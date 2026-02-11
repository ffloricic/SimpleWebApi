using SimpleWebApi.Abstractions.Middleware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleWebApi.Abstractions.Routing
{
    public interface IEndpoint
    {
        HttpMethod Method { get; }
        string PathPattern { get; }
        RequestDelegate Handler { get; }
    }
}
