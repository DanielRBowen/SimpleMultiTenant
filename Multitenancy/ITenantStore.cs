using System.Threading.Tasks;

namespace Multitenancy
{
    public interface ITenantStore<T> where T : Tenant
    {
        Task<T> GetTenantAsync(string identifier, bool isPath = false);
    }
}
