using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CFOP.Server.Repository;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;

namespace CFOP.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            var hostingEnvironment = Resolve<IHostingEnvironment>(host);
            if (hostingEnvironment.IsDevelopment())
            {
                var dbContext = Resolve<ServerContext>(host);
                DbInitializer.Seed(dbContext);
            }

            host.Run();
        }

        private static T Resolve<T>(IWebHost host)
        {
            return (T)host.Services.GetService(typeof(T));
        }
    }
}
