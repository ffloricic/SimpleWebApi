using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleWebApi.Abstractions.Http
{
    public interface IResponse
    {
        int StatusCode { get; set; }
        IDictionary<string, IList<string>> Headers { get; }
        public Stream Body { get; }
    }
}
