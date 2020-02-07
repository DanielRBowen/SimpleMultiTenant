# SimpleMultiTenant
A simple multi-tenant app with starting code from: https://michael-mckenna.com/multi-tenant-asp-dot-net-core-application-tenant-resolution/

Trying to incorporate code from Azure [multitenant example](https://docs.microsoft.com/en-us/azure/sql-database/saas-dbpertenant-wingtip-app-overview#sql-database-wingtip-saas-tutorials]):
```
git clone --single-branch --branch Upgrade-VS17 https://github.com/microsoft/WingtipTicketsSaaS-DbPerTenant.git
```

and from [SaasKit](https://github.com/saaskit/saaskit/tree/dev)
```
git clone --single-branch --branch dev https://github.com/saaskit/saaskit.git
```

Trying for the tenant identifier to be the subdomain. And trying for easy debugging using localdb instead of an azure database in an elastic pool.
A nice addition would be to be able to dynamically create new tenants from an administrative site and change content and themes per tenant.
