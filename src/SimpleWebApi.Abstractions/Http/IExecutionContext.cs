using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleWebApi.Abstractions.Http
{
    public interface IExecutionContext: IContext
    {
        IDictionary<string, object> Items { get; }
    }

}
