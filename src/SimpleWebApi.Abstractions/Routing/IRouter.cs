using SimpleWebApi.Abstractions.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleWebApi.Abstractions.Routing
{
    public interface IRouter
    {
        Task<IEndpoint?> MatchAsync(IExecutionContext context);
    }
}
