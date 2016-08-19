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

            var dbContext = (ServerContext)host.Services.GetService(typeof(ServerContext));
            DbInitializer.Seed(dbContext);

            host.Run();
        }
    }
}
