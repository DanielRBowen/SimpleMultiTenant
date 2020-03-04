using Domain.Tenants.Data;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Domain.Tenants.Multitenancy
{
    public class TenantsDbStore : ITenantStore<Tenant>
    {
        private readonly TenantsDbContext _tenantsDbContext;
        private readonly IConfiguration _configuration;

        public TenantsDbStore(TenantsDbContext tenantsDbContext, IConfiguration configuration)
        {
            _tenantsDbContext = tenantsDbContext;
            _configuration = configuration;
        }

        public async Task<Tenant> GetTenantAsync(string identifier)
        {
            Tenant tenant = null;

            if (string.IsNullOrWhiteSpace(identifier))
            {
                tenant = GetDefaultTenant();
            }
            else
            {
                tenant = _tenantsDbContext.Tenants.SingleOrDefault(tenant => tenant.Name.ToLower() == identifier.ToLower());
            }

            if (tenant == null)
            {
                tenant = GetDefaultTenant();
            }

            if (tenant == null)
            {
                throw new NullReferenceException($"The path: {identifier}, does not contain any of the tenant ids.");
            }

            return await Task.FromResult(tenant);
        }

        private Tenant GetDefaultTenant()
        {
            var defaultTenantName = _configuration.GetValue<string>("DefaultTenant");
            return _tenantsDbContext.Tenants.SingleOrDefault(tenant => tenant.Name == defaultTenantName);
        }
    }
}
