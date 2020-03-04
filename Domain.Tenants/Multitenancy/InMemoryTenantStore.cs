using System;
using System.Linq;
using System.Threading.Tasks;

namespace Domain.Tenants.Multitenancy
{
    /// <summary>
    /// In memory store for testing
    /// </summary>
    public class InMemoryTenantStore : ITenantStore<Tenant>
    {
        private readonly MultitenantConfiguration _multitenantConfiguration;

        public InMemoryTenantStore(MultitenantConfiguration multitenantConfiguration)
        {
            _multitenantConfiguration = multitenantConfiguration;

            if (multitenantConfiguration?.Tenants == null || multitenantConfiguration.Tenants.Any() == false)
            {
                throw new NullReferenceException("There are no tenants in the multitenant configuration.");
            }
        }

        /// <summary>
        /// Get a tenant for a given identifier
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public async Task<Tenant> GetTenantAsync(string identifier)
        {
            Tenant tenant = null;

            if (string.IsNullOrWhiteSpace(identifier))
            {
                tenant = GetDefaultTenant();
            }
            else
            {
                tenant = _multitenantConfiguration.Tenants.SingleOrDefault(tenant => tenant.Name.ToLowerInvariant() == identifier.ToLowerInvariant());
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
            return _multitenantConfiguration.Tenants.SingleOrDefault(tenant => tenant.Name.ToLowerInvariant() == _multitenantConfiguration.DefaultTenant.ToLowerInvariant());
        }
    }
}
