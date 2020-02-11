using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using SimpleMultiTenant.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Multitenancy;
using SimpleMultiTenant.Api;
using Autofac;

namespace SimpleMultiTenant
{
    public class Startup
    {

        public static IConfiguration Configuration { get; set; }

        public static List<Tenant> InMemoryTenants { get; set; } = new List<Tenant>();

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var connectionStrings = Configuration.GetSection("ConnectionStrings").GetChildren().ToDictionary(pair => pair.Key, pair => pair.Value);

            foreach (var connectionString in connectionStrings)
            {
                var connectionStringValue = connectionString.Value;

                var tenant = new Tenant
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = connectionString.Key,
                    ConnectionString = connectionStringValue
                };

                InMemoryTenants.Add(tenant);
                var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
                var options = optionsBuilder.UseSqlServer(connectionStringValue).Options; 
                var dbContext = new ApplicationDbContext(options);
                dbContext.Database.Migrate();
            }

            services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddControllersWithViews();
            services.AddRazorPages();

            services.AddSingleton<IEnumerable<Tenant>>(tenants => InMemoryTenants);
            services.AddMultiTenancy()
              .WithResolutionStrategy<HostResolutionStrategy>()
              .WithStore<InMemoryTenantStore>();
        }

        public static void ConfigureMultiTenantServices(Tenant tenant, ContainerBuilder containerBuilder)
        {
            //c.Register...
            containerBuilder.RegisterInstance(new OperationIdService()).SingleInstance();
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            var options = optionsBuilder.UseSqlServer(Configuration.GetConnectionString(tenant.Name)).Options;
            containerBuilder.RegisterInstance(new ApplicationDbContext(options)).SingleInstance();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseMultiTenancy().UseMultiTenantContainer();
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{tenant?}/{controller=Home}/{action=Index}/{id?}");

                endpoints.MapRazorPages();
            });
        }
    }
}
