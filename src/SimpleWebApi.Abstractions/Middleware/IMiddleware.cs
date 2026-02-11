using SimpleWebApi.Abstractions.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleWebApi.Abstractions.Middleware
{
    public interface IMiddleware
    {
        Task InvokeAsync(IExecutionContext context, RequestDelegate next);
    }
}
