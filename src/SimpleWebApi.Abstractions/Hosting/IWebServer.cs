using SimpleWebApi.Abstractions.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SimpleWebApi.Abstractions.Hosting
{
    public interface IWebServer
    {
        /// <summary>
        /// Starts accepting incoming connections.
        /// This method MUST NOT wait for request execution to complete.
        /// </summary>
        Task StartAsync(
            Func<ITransportContext, Task> requestHandler,
            CancellationToken cancellationToken);
    }
}
