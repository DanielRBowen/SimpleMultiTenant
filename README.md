# SimpleMultiTenant
A simple multi-tenant app with starting code from: [Michael Mckenna's blog](https://michael-mckenna.com/multi-tenant-asp-dot-net-core-application-tenant-resolution/) His Github is [here](https://github.com/myquay)

## The Simple most recent way to do Multitenancy:
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

You have to change the scaffolded Identity UI to get Identity to work with each tenant. Like suggested in the top answer [here](https://stackoverflow.com/questions/50682108/change-routing-in-asp-net-core-identity-ui)

So you have to remove the AddDefaultUI from AddIdentityCore in Configure service like this answer [here](https://stackoverflow.com/questions/51138449/no-accountcontroller-for-asp-net-core-2-1)

In my production project I changed the scaffolded Identity UI Pages into a MVC version. The login and register doesn't work. I am guessing because of the cookes for each tenant needs to be configured.

## Other dated ways to do Multitenancy
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
