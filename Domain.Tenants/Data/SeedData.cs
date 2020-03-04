using Domain.Tenants.Multitenancy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Domain.Tenants.Data
{
    public static class SeedData
    {
        public static void SeedTenants(Dictionary<string, string> connectionStrings, TenantsDbContext tenantsDbContext)
        {
            foreach (var connectionString in connectionStrings)
            {
                var tenantName = connectionString.Key;
                var tenantConnectionString = connectionString.Value;

                if (tenantsDbContext.Tenants.Any(tenant => tenant.Name == tenantName))
                {
                    continue;
                }
                else
                {
                    var newTenant = new Tenant
                    {
                        Guid = Guid.NewGuid().ToString(),
                        Name = tenantName,
                        ConnectionString = tenantConnectionString
                    };

                    tenantsDbContext.Tenants.Add(newTenant);
                }
            }

            tenantsDbContext.SaveChanges();
        }
    }
}
