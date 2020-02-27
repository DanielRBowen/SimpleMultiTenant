# SimpleMultiTenant
A simple multi-tenant app with starting code from: [Michael Mckenna's blog](https://michael-mckenna.com/multi-tenant-asp-dot-net-core-application-tenant-resolution/) His Github is [here](https://github.com/myquay)

## The Simple most recent way to do Multitenancy:
The tenant is resolved by the first path route parameter.
```
HttpContext.Request.Path.Value.Split('/');
```

So the tenant is Mapped in startup like this:
```
endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{tenant}/{controller=Home}/{action=Index}/{id?}");
```

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
