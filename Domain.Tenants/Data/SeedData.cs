using Domain.Tenants.Multitenancy;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

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


            var newTenant1 = new Tenant
            {
                Guid = Guid.NewGuid().ToString(),
                Name = "Aleidy",
                ConnectionString = "\"Aleidy\": \"Server=(localdb)\\\\mssqllocaldb;Database=Aleidy;Trusted_Connection=True;MultipleActiveResultSets=true\"",
                DomainNames = "aleidy.com",
                IpAddresses = "233.106.33.141, 33.225.61.124"
            };

            if (tenantsDbContext.Tenants.Any(tenant => tenant.Name == newTenant1.Name) == false)
            {
                tenantsDbContext.Tenants.Add(newTenant1);
            }

            var newTenant2 = new Tenant
            {
                Guid = Guid.NewGuid().ToString(),
                Name = "Oiscus",
                ConnectionString = @"""Oiscus"": ""Server=(localdb)\\mssqllocaldb;Database=Oiscus;Trusted_Connection=True;MultipleActiveResultSets=true""",
                DomainNames = "oiscus.com, slonds.com",
                IpAddresses = "97.198.174.206, 216.204.170.148"
            };

            if (tenantsDbContext.Tenants.Any(tenant => tenant.Name == newTenant2.Name) == false)
            {
                tenantsDbContext.Tenants.Add(newTenant2);
            }

            tenantsDbContext.SaveChanges();
            WriteTenantsToFile(tenantsDbContext.Tenants.ToList());
        }

        private static void WriteTenantsToFile(IEnumerable<Tenant> tenants)
        {
            var contents = JsonConvert.SerializeObject(tenants, Formatting.Indented);
            File.WriteAllText(Directory.GetCurrentDirectory() + "/customtenants.json", contents);
        }
    }
}
