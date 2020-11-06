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
using SimpleMultiTenant.Api;
using SimpleMultiTenant.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using SimpleMultiTenant.Security;
using Domain.Tenants.Data;
using Domain.Tenants.Multitenancy;
using SimpleMultiTenant.FileManagement;
using System.IO;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace SimpleMultiTenant
{
    public class Startup
    {

        public static IConfiguration Configuration { get; set; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<TenantsDbContext>(options =>
               options.UseSqlServer(
                   Configuration.GetConnectionString("TenantsSimple")));

            services.AddScoped<ApplicationDbContext>();
            MigrateTenantsDatabases();

            services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.ConfigureApplicationCookie(options =>
            {
                options.ReturnUrlParameter = "/";
                options.Events = new CookieAuthenticationEvents
                {
                    OnRedirectToAccessDenied = context =>
                    {
                        var tenantName = context.HttpContext.GetTenant().Name;

                        if (Configuration.GetValue<bool>("UsePathToResolveTenant"))
                        {
                            context.Response.Redirect(new PathString($"/{tenantName}/Account/Login"));
                        }
                        else
                        {
                            context.Response.Redirect(new PathString("/Account/Login"));
                        }

                        return Task.CompletedTask;
                    }
                };
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy(PolicyNames.RequireTenant,
                    policy =>
                    {
                        policy.AddRequirements(new InCurrentTenantRequirement(new HttpContextAccessor()));
                        policy.RequireAuthenticatedUser();
                    });
            });

            services.AddControllersWithViews();
            services.AddRazorPages();

            if (Configuration.GetValue<bool>("UseRedis"))
            {
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = Configuration["RedisConfig:Configuration"];
                    options.InstanceName = Configuration["RedisConfig:InstanceName"];
                });
            }
            else
            {
                services.AddDistributedMemoryCache();
            }

            services.AddScoped<ISettingsService, SettingsService>();

            services.AddMultiTenancy()
              .WithResolutionStrategy<HostResolutionStrategy>()
              .WithStore<TenantsDbStore>();
        }

        private void MigrateTenantsDatabases()
        {
            // Instead of using the connections.json file. Managing tenants from the TenantsDbContext will work too.
            // var tenants = new TenantsDbContext(options).Tenants.ToList();
            var connectionStrings = GetConnectionStrings();

            foreach (var connectionString in connectionStrings)
            {
                var connectionStringValue = connectionString.Value;
                var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
                var options = optionsBuilder.UseSqlServer(connectionStringValue).Options;
                var dbContext = new ApplicationDbContext(options);
                dbContext.Database.Migrate();
                 IUserStore<IdentityUser> store = new UserStore<IdentityUser>(dbContext);
                var userManager = new UserManager<IdentityUser>(store, null, new PasswordHasher<IdentityUser>(), null, null, null, null, null, null);
                var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(dbContext), null, null, null, null);
                Data.SeedData.Seed(dbContext, userManager, roleManager, new string[] { "admin" }).Wait();
                TenantsCustomFolderManager.CreateContentDirectoryIfItDoesNotExist(Directory.GetCurrentDirectory() + "/wwwroot/tenants/", connectionString.Key);
            }
        }

        private Dictionary<string, string> GetConnectionStrings()
        {
            var connectionStrings = Configuration.GetSection("ConnectionStrings").GetChildren().ToDictionary(pair => pair.Key, pair => pair.Value);
            connectionStrings.Remove("TenantsSimple");
            return connectionStrings;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, TenantsDbContext tenantsDbContext)
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

            app.UseMultiTenancy();
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                if (Configuration.GetValue<bool>("UsePathToResolveTenant"))
                {
                    endpoints.MapControllerRoute(
                        name: "default",
                        pattern: "{tenant}/{controller=Home}/{action=Index}/{id?}");
                }
                else
                {
                    endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                }

                endpoints.MapRazorPages();
            });

            tenantsDbContext.Database.Migrate();
            Domain.Tenants.Data.SeedData.SeedTenants(GetConnectionStrings(), tenantsDbContext);
            CustomTenantsFileManager.AddAnyNewCustomDomainsOrIps(tenantsDbContext);
        }
    }
}
