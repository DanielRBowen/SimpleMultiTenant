using System.Threading.Tasks;

namespace Multitenancy
{
    public interface ITenantResolutionStrategy
    {
        Task<string> GetTenantIdentifierAsync();
    }
}
