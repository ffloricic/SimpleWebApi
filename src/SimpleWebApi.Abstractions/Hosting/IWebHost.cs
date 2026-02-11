using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleWebApi.Abstractions.Hosting
{
    public interface IWebHost
    {
        Task StartAsync(CancellationToken cancellationToken);
    }

}
