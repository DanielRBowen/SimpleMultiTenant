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

On Controllers which have an [ApiController] attribute it will also need a route attribute with the tenant. And also have the non tenant one so that the subdomain works.
```
[ApiController]
[Route("{tenant?}/api/[controller]")]
[Route("api/[controller]")]
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

When getting a token for authenticated API calls you will need to sign in with the claims principle which are going to be added to the token so that the claims show in the InCurrentTenantRequirment Like so:

```
await _signInManager.Context.SignInAsync(IdentityConstants.ApplicationScheme,
                           principal,
                           new AuthenticationProperties { IsPersistent = false });
```

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

## Tenant resolved by subdomain or path?
I have modified this simple example to only resolve as a path when in debug mode and be resolved as a subdomain in release.
[This](https://stackoverflow.com/questions/4987201/why-use-subdomains-to-designate-tenants-in-a-multi-tenant-web-application) is a StackOverflow question which is asking if subdomain or path is better I think it is a good thing to figure out how to do both like the comments suggest:
>As a recommendation, I would say design you app from the outset not to use subdomains, and then build this functionality in as a final layer. If you integrate subdomains all the way through, it becomes very inflexible to change it in future. (source: experience) 


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

## For Redis Cache
I installed Ubuntu 18.04 LTS from the Microsoft Store
Turn on "Windows Subsystem for Linux" in the Windows Features in the Windows Control panel then restart the computer.
Open up Ubuntu and set a username and password
In the Ubuntu shell do: 
```
sudo apt update && sudo apt upgrade
```
Then:
```
sudo apt install redis-server
```
To restart the redis server
```
sudo service redis-server restart
```
Then open the cli for redis
```
redis-cli
```
In the cli
```
client getname
client setname eow
```
In the appsettings.json set the Configuration of the RedisConfig to the redis IP and portnumber such as 127.0.0.1:6379 and the InstanceName to the name you just set.

To see all the keys type
```
keys *
```
