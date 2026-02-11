using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleWebApi.Abstractions.Middleware
{
    public delegate Task RequestDelegate(ExecutionContext context);
}
