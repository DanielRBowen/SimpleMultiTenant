using System.Threading.Tasks;

namespace Domain.Tenants.Multitenancy
{
    public interface ITenantResolutionStrategy
    {
        Task<(string domainName, string ipAddresss, string name)> GetTenantIdentifierAsync();
    }
}
