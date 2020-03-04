using System.Threading.Tasks;

namespace Domain.Tenants.Multitenancy
{
    public interface ITenantStore<T> where T : Tenant
    {
        Task<T> GetTenantAsync(string identifier);
    }
}
