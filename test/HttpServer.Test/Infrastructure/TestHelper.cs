using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpServer.Test.Infrastructure
{
    public static class TestHelper
    {
        public static void Log(string message)
        {
            Debug.WriteLine($"{DateTime.Now.Hour}:{DateTime.Now.Minute}:{DateTime.Now.Second},{DateTime.Now.Microsecond} - {message}, Thread: {Thread.CurrentThread.Name}");
        }
    }
}
