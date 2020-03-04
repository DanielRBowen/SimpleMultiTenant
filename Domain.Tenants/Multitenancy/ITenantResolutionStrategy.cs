using System.Threading.Tasks;

namespace Domain.Tenants.Multitenancy
{
    public interface ITenantResolutionStrategy
    {
        Task<string> GetTenantIdentifierAsync();
    }
}
