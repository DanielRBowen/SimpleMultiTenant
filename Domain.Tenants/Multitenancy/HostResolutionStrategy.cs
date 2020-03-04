using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Domain.Tenants.Multitenancy
{
    /// <summary>
    /// Resolve the host to a tenant identifier
    /// </summary>
    public class HostResolutionStrategy : ITenantResolutionStrategy
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HostResolutionStrategy(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Get the tenant identifier
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<string> GetTenantIdentifierAsync()
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext == null)
            {
                return await Task.FromResult(string.Empty);
            }
            else
            {
                var path = await Task.FromResult(httpContext.Request.Path);

                if (path.HasValue)
                {
                    var tenantIdentifier = path.Value.Split('/')[1];
                    return await Task.FromResult(tenantIdentifier);
                }
                else
                {
                    return await Task.FromResult(string.Empty);
                }
            }
        }
    }
}
