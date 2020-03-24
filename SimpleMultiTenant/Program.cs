using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SimpleMultiTenant.Data;

namespace SimpleMultiTenant
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile(Directory.GetCurrentDirectory() + "/connections.json", optional: false, reloadOnChange: true);
#if DEBUG
                    config.AddJsonFile(Directory.GetCurrentDirectory() + "/../Domain.Tenants/tenantsconfig.json", optional: false, reloadOnChange: true);
#else
                    config.AddJsonFile(Directory.GetCurrentDirectory() + "/tenantsconfig.json", optional: false, reloadOnChange: true);
#endif 
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
