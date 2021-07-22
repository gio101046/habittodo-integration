using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.Extensions.Hosting;

namespace Habittodo.FunctionApp
{
    public class Program
    {
        public static void Main()
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults()
                .Build();
            host.Run();
        }
    }
}