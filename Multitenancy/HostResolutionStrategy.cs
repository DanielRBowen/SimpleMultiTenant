using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Multitenancy
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
            if (_httpContextAccessor.HttpContext == null)
            {
                return await Task.FromResult(string.Empty);
            }
            else
            {
                var path = await Task.FromResult(_httpContextAccessor.HttpContext.Request.Path);

                if (path.HasValue)
                {
                    return await Task.FromResult(path.Value.Split('/')[1]);
                }
                else
                {
                    return await Task.FromResult(string.Empty);
                }
            }
        }
    }
}
