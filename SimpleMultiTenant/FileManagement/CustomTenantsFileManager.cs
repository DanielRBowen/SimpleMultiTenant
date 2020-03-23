using Domain.Tenants.Data;
using Domain.Tenants.Multitenancy;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SimpleMultiTenant.FileManagement
{
    public class CustomTenantsFileManager
    {
        private static readonly string s_customTenantsFilePath = Directory.GetCurrentDirectory() + "/customtenants.json";
        /// <summary>
        /// Should be ran at startup
        /// </summary>
        /// <param name="tenantsDbContext"></param>
        public static void AddAnyNewCustomDomainsOrIps(TenantsDbContext tenantsDbContext)
        {
            if (tenantsDbContext == null)
            {
                return;
            }

            try
            {
                if (File.Exists(s_customTenantsFilePath))
                {
                    var tenants = tenantsDbContext.Tenants.ToList();
                    var customTenants = JsonConvert.DeserializeObject<List<Tenant>>(File.ReadAllText(s_customTenantsFilePath));

                    foreach (var customTenant in customTenants)
                    {
                        if (tenants.Any(tenant => tenant.Name == customTenant.Name && tenant.DomainNames != customTenant.DomainNames))
                        {
                            var updatedTenant = tenants.SingleOrDefault(tenant => tenant.Name == customTenant.Name);
                            updatedTenant.DomainNames = customTenant.DomainNames;
                            tenantsDbContext.Update(updatedTenant);
                        }

                        if (tenants.Any(tenant => tenant.Name == customTenant.Name && tenant.IpAddresses != customTenant.IpAddresses))
                        {
                            var updatedTenant = tenants.SingleOrDefault(tenant => tenant.Name == customTenant.Name);
                            updatedTenant.IpAddresses = customTenant.IpAddresses;
                            tenantsDbContext.Update(updatedTenant);
                        }
                    }

                    tenantsDbContext.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void UpdateCustomTenant(Tenant updatedTenant)
        {
            var customTenantsJArray = JArray.Parse(File.ReadAllText(s_customTenantsFilePath));
            var oldJToken = customTenantsJArray.FirstOrDefault(jToken => (string)jToken["Name"] == updatedTenant.Name);
            customTenantsJArray.Remove(oldJToken);

            var jsonMergeSettings = new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Union,
                MergeNullValueHandling = MergeNullValueHandling.Merge,
                PropertyNameComparison = StringComparison.InvariantCulture
            };

            customTenantsJArray.Add(JToken.FromObject(new { updatedTenant.Name, updatedTenant.IpAddresses, updatedTenant.DomainNames }));
            File.WriteAllText(s_customTenantsFilePath, customTenantsJArray.ToString());
        }

        public static void RemoveCustomTenant(string tenantName)
        {
            var customTenantsJArray = JArray.Parse(File.ReadAllText(s_customTenantsFilePath));
            var oldJToken = customTenantsJArray.FirstOrDefault(jToken => (string)jToken["Name"] == tenantName);
            customTenantsJArray.Remove(oldJToken);
            File.WriteAllText(s_customTenantsFilePath, customTenantsJArray.ToString());
        }
    }
}
