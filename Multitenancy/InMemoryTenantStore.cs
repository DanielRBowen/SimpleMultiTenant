using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multitenancy
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
            var identifierLower = identifier.ToLowerInvariant();
            Tenant tenant = null;
            tenant = _multitenantConfiguration.Tenants.SingleOrDefault(tenant => identifierLower.Contains(tenant.Name.ToLowerInvariant()));

            if (tenant == null)
            {
                tenant = _multitenantConfiguration.Tenants.SingleOrDefault(tenant => tenant.Name.ToLowerInvariant() == _multitenantConfiguration.DefaultTenant.ToLowerInvariant());
            }

            if (tenant == null)
            {
                throw new NullReferenceException($"The path: {identifierLower}, does not contain any of the tenant ids.");
            }

            return await Task.FromResult(tenant);
        }
    }
}
