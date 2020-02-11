using System.Linq;
using System.Threading.Tasks;

namespace Multitenancy
{
    /// <summary>
    /// In memory store for testing
    /// </summary>
    public class InMemoryTenantStore : ITenantStore<Tenant>
    {
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

            var inMemoryTenants = new[]
            {
                new Tenant{ Id = "80fdb3c0-5888-4295-bf40-ebee0e3cd8f3", Name = "localhost" },
                new Tenant{ Id = "80fdb3c0-5888-4295-bf40-ebee0e3cd8f2", Name = "t01" },
                new Tenant{ Id = "80fdb3c0-5888-4295-bf40-ebee0e3cd8f1", Name = "t02" }
            };

            Tenant tenant = null;

            if (isPath == true)
            {
                tenant = inMemoryTenants.SingleOrDefault(t => identifier.Contains(t.Name));
            }
            else
            {
                tenant = inMemoryTenants.SingleOrDefault(t => t.Name == identifier);
            }

            if (tenant == null)
            {
                return inMemoryTenants.FirstOrDefault();
            }

            return await Task.FromResult(tenant);
        }
    }
}
