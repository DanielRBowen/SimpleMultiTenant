# SimpleMultiTenant
A simple multi-tenant app with starting code from the first part of: [Michael Mckenna's blog](https://michael-mckenna.com/multi-tenant-asp-dot-net-core-application-tenant-resolution/) His Github is [here](https://github.com/myquay)

## A simple way to do Multitenancy:
The tenant is resolved by the first path route parameter.
```
HttpContext.Request.Path.Value.Split('/')[1];
```

So the tenant is Mapped in startup like this:
```
endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{tenant}/{controller=Home}/{action=Index}/{id?}");
```

You will need to add the tenant name for javascript http requests so in Javascript you can get the tenant with this:
```
tenantName: '/' + window.location.pathname.split('/')[1]
```

On Controllers which have an [ApiController] attribute it will also need a route attribute with the tenant.
```
[ApiController]
[Route("{tenant}/[controller]")]
```

You have to painfully change the scaffolded Identity UI into a custom MVC implementation with an account controller and views to get Identity to work with each tenant. Like suggested in the top answer [here](https://stackoverflow.com/questions/50682108/change-routing-in-asp-net-core-identity-ui)

Instead of adding AddDefaultIdentity(), the AddIdentity() should be added in Configure services.

AddIdentity Example:
```
 services.AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = false)
                .AddSignInManager()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
```

If you get an ANCM Multiple In-Process Applications in same Process Error when using different tenant than the default tenant then delete the .vs folder in the folder near your sln file like it says [here](https://stackoverflow.com/questions/58246822/http-error-500-35-ancm-multiple-in-process-applications-in-same-process-asp-ne)

[This](https://docs.microsoft.com/en-us/azure/architecture/multitenant-identity/) explains how to do identity in multitenant applications.

The application will need to only have the authorize user see his tenant's site. A claim can be added while logging on and that claim can be checked on controllers and methods with a ActionFilterAttribute or be checked with an authorize policy. [This](https://blog.dangl.me/archive/adding-custom-claims-when-logging-in-with-aspnet-core-identity-cookie/) is how you can add a tenant id to the claims on the login process. [Here](https://github.com/DanielRBowen/SimpleMultiTenant/blob/master/SimpleMultiTenant/Attributes/IsUserInCurrentTenantAttribute.cs) is the ActionFilterAttribute to use on controllers without authorize attributes. And [this](https://github.com/DanielRBowen/SimpleMultiTenant/blob/master/SimpleMultiTenant/Security/InCurrentTenantRequirement.cs) is the Authorize requirement.

This is how to configure application cookie to redirect to login when access is denied:

```
  services.ConfigureApplicationCookie(options =>
            {
                options.ReturnUrlParameter = "/";
                options.Events = new CookieAuthenticationEvents
                {
                    OnRedirectToAccessDenied = context =>
                    {
                        var tenantName = context.HttpContext.GetTenant().Name;
                        context.Response.Redirect(new PathString($"/{tenantName}/Account/Login"));
                        return Task.CompletedTask;
                    }
                };
            });
```

This implementation add the tenantsDbContext and also add the applicationDbContext in ConfigureServices for dependency injection:
 ```
 services.AddDbContext<TenantsDbContext>(options =>
               options.UseSqlServer(
                   Configuration.GetConnectionString("TenantsSimple")));

            services.AddScoped<ApplicationDbContext>();
 ```

For the DbContext constructor add the connection string:
```
 public ApplicationDbContext(IHttpContextAccessor httpContextAccessor, TenantsDbContext tenantsDbContext)
           : base(CreateDbContextOptions(httpContextAccessor, tenantsDbContext))
        {
        }

        private static DbContextOptions CreateDbContextOptions(IHttpContextAccessor httpContextAccessor, TenantsDbContext tenantsDbContext)
        {
            var tenantName = httpContextAccessor.HttpContext.GetTenant().Name;
            var connectionString = tenantsDbContext.Tenants.SingleOrDefault(tenant => tenant.Name == tenantName).ConnectionString;

            if (connectionString == null)
            {
                throw new NullReferenceException($"The connection string was null for the tenant: {tenantName}");
            }

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            return optionsBuilder.UseSqlServer(connectionString).Options;
        }
```

## Other Multitenancy examples
(1)
From Azure [multitenant example](https://docs.microsoft.com/en-us/azure/sql-database/saas-dbpertenant-wingtip-app-overview#sql-database-wingtip-saas-tutorials]):
```
git clone --single-branch --branch Upgrade-VS17 https://github.com/microsoft/WingtipTicketsSaaS-DbPerTenant.git
```

and

(2)
From [SaasKit](https://github.com/saaskit/saaskit/tree/dev)
```
git clone --single-branch --branch dev https://github.com/saaskit/saaskit.git
```
