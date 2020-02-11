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
        private readonly List<Tenant> _tenants;
        public InMemoryTenantStore(IEnumerable<Tenant> tenants)
        {
            _tenants = tenants.ToList();
        }
        /// <summary>
        /// Get a tenant for a given identifier
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public async Task<Tenant> GetTenantAsync(string identifier, bool isPath = false)
        {
            if (string.IsNullOrWhiteSpace(identifier))
            {
                return null;
            }

            var identifierLower = identifier.ToLowerInvariant();

            Tenant tenant = null;

            if (isPath == true)
            {
                tenant = _tenants.SingleOrDefault(tenant => identifierLower.Contains(tenant.Name.ToLowerInvariant()));
            }
            else
            {
                tenant = _tenants.SingleOrDefault(tenant => tenant.Name.ToLowerInvariant() == identifierLower);
            }

            if (tenant == null)
            {
                return _tenants.FirstOrDefault();
            }

            return await Task.FromResult(tenant);
        }
    }
}
